using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;
using MYAcademy_IdentityEmailProject.Services;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    [AllowAnonymous]
    public class RegisterController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(UserManager<AppUser> userManager, IEmailService emailService, ILogger<RegisterController> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Dashboard");
            return View(new UserRegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserRegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var appUser = new AppUser
            {
                Name = model.Name.Trim(),
                Surname = model.Surname.Trim(),
                Email = model.Email.Trim(),
                UserName = model.Username.Trim(),
                ValidCode = NewCode()
            };

            var result = await _userManager.CreateAsync(appUser, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            var sent = await TrySendCodeAsync(appUser);
            TempData["Toast"] = sent
                ? "Hesabınız oluşturuldu. Email adresinize gönderilen 6 haneli kodu girin."
                : "Hesabınız oluşturuldu ancak doğrulama kodu gönderilemedi. 'Kodu tekrar gönder' ile deneyin.";
            return RedirectToAction(nameof(ConfirmMail), new { email = appUser.Email });
        }

        [HttpGet]
        public IActionResult ConfirmMail(string? email)
        {
            return View(new ConfirmMailViewModel { Email = email ?? string.Empty });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmMail(ConfirmMailViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Doğrulama kodu hatalı.");
                return View(model);
            }

            if (user.EmailConfirmed)
            {
                TempData["Toast"] = "Hesabınız zaten doğrulanmış. Giriş yapabilirsiniz.";
                return RedirectToAction("SignIn", "Login");
            }

            // Kaba kuvvet denemelerine karşı: 5 hatalı denemede hesap 5 dakika kilitlenir
            if (await _userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Çok fazla hatalı deneme yapıldı. Birkaç dakika sonra tekrar deneyin.");
                return View(model);
            }

            if (user.ValidCode == null || user.ValidCode.ToString() != model.Code.Trim())
            {
                await _userManager.AccessFailedAsync(user);
                ModelState.AddModelError(string.Empty, "Doğrulama kodu hatalı.");
                return View(model);
            }

            user.EmailConfirmed = true;
            user.ValidCode = null;
            await _userManager.UpdateAsync(user);
            await _userManager.ResetAccessFailedCountAsync(user);

            TempData["Toast"] = "Email adresiniz doğrulandı. Şimdi giriş yapabilirsiniz.";
            return RedirectToAction("SignIn", "Login");
        }

        [HttpPost]
        public async Task<IActionResult> ResendCode(string email)
        {
            var user = string.IsNullOrWhiteSpace(email) ? null : await _userManager.FindByEmailAsync(email.Trim());

            if (user != null && !user.EmailConfirmed)
            {
                user.ValidCode = NewCode();
                await _userManager.UpdateAsync(user);
                await TrySendCodeAsync(user);
            }

            // Hesabın var olup olmadığını belli etmemek için her durumda aynı mesaj
            TempData["Toast"] = "Hesap doğrulanmamışsa yeni bir kod gönderildi. Gelen kutunuzu kontrol edin.";
            return RedirectToAction(nameof(ConfirmMail), new { email });
        }

        // Kayıt formundaki anlık kullanıcı adı kontrolü
        [HttpGet]
        public async Task<IActionResult> CheckUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return Json(new { available = false });
            var exists = await _userManager.FindByNameAsync(username.Trim()) != null;
            return Json(new { available = !exists });
        }

        private static int NewCode() => RandomNumberGenerator.GetInt32(100000, 1000000);

        private async Task<bool> TrySendCodeAsync(AppUser user)
        {
            try
            {
                await _emailService.SendAsync(user.Email!, "Mailora · Email Doğrulama Kodu",
                    EmailTemplates.VerificationCode(user.Name ?? user.UserName ?? "", user.ValidCode ?? 0));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Doğrulama kodu gönderilemedi: {Email}", user.Email);
                return false;
            }
        }
    }
}
