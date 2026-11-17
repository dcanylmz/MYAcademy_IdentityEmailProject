using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;
using MYAcademy_IdentityEmailProject.Services;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager,
            IEmailService emailService, ILogger<LoginController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult SignIn(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Dashboard");
            ViewData["ReturnUrl"] = returnUrl;
            return View(new UserLoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(UserLoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var login = model.Username.Trim();
            var user = login.Contains('@')
                ? await _userManager.FindByEmailAsync(login)
                : await _userManager.FindByNameAsync(login);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
                return RedirectToAction("Index", "Dashboard");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Çok fazla hatalı deneme yapıldı. Hesabınız 5 dakika kilitlendi.");
                return View(model);
            }

            // RequireConfirmedEmail = true → email doğrulanmamışsa NotAllowed döner.
            // Şifre doğruysa kullanıcıyı doğrulama ekranına yönlendir.
            if (result.IsNotAllowed && !user.EmailConfirmed && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                TempData["Toast"] = "Giriş yapabilmek için önce email adresinizi doğrulamalısınız.";
                return RedirectToAction("ConfirmMail", "Register", new { email = user.Email });
            }

            ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Toast"] = "Oturumunuz kapatıldı.";
            return RedirectToAction(nameof(SignIn));
        }

        // ---------------- Şifremi Unuttum ----------------

        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var link = Url.Action(nameof(ResetPassword), "Login", new { email = user.Email, token }, Request.Scheme)!;
                try
                {
                    await _emailService.SendAsync(user.Email!, "Mailora · Şifre Yenileme",
                        EmailTemplates.ResetPassword(user.Name ?? user.UserName ?? "", link));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Şifre yenileme e-postası gönderilemedi: {Email}", user.Email);
                }
            }

            // Hesabın var olup olmadığı belli edilmez
            ViewData["Sent"] = true;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string? email, string? token)
        {
            ViewData["InvalidLink"] = !await IsResetTokenValidAsync(email, token);
            return View(new ResetPasswordViewModel { Email = email ?? "", Token = token ?? "" });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = string.IsNullOrEmpty(model.Email) ? null : await _userManager.FindByEmailAsync(model.Email);
            if (user == null || string.IsNullOrEmpty(model.Token))
            {
                ViewData["InvalidLink"] = true;
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                ViewData["InvalidLink"] = false;
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                // Token kullanılmış veya süresi dolmuşsa formu değil uyarıyı göster
                ViewData["InvalidLink"] = result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.InvalidToken));
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            // Bağlantı email'e geldiği için adres sahipliği de kanıtlanmış olur
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                user.ValidCode = null;
                await _userManager.UpdateAsync(user);
            }
            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, null);

            TempData["Toast"] = "Şifreniz güncellendi. Yeni şifrenizle giriş yapabilirsiniz.";
            return RedirectToAction(nameof(SignIn));
        }

        private async Task<bool> IsResetTokenValidAsync(string? email, string? token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token)) return false;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;
            return await _userManager.VerifyUserTokenAsync(user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                UserManager<AppUser>.ResetPasswordTokenPurpose, token);
        }
    }
}
