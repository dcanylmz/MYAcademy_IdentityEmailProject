using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;
using MYAcademy_IdentityEmailProject.Services;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    public class ProfileController : Controller
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxImageBytes = 2 * 1024 * 1024;   // 2 MB

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly EmailContext _context;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            EmailContext context, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            return View(await BuildModelAsync(user));
        }

        // Kişisel bilgiler (ad, soyad, hakkımda)
        [HttpPost]
        public async Task<IActionResult> Index([Bind(Prefix = "Edit")] ProfileEditViewModel edit)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                var model = await BuildModelAsync(user);
                model.Edit = edit;
                return View(model);
            }

            user.Name = edit.Name.Trim();
            user.Surname = edit.Surname.Trim();
            user.Description = edit.Description?.Trim();
            await _userManager.UpdateAsync(user);

            TempData["Toast"] = "Profil bilgileriniz güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // Profil fotoğrafı yükleme
        [HttpPost]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadPhoto(IFormFile? photo)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var ext = Path.GetExtension(photo?.FileName ?? "").ToLowerInvariant();
            if (photo == null || photo.Length == 0)
                TempData["Toast"] = "Lütfen bir fotoğraf seçin.";
            else if (!AllowedImageExtensions.Contains(ext) || !(photo.ContentType ?? "").StartsWith("image/"))
                TempData["Toast"] = "Yalnızca JPG, PNG, WEBP veya GIF yükleyebilirsiniz.";
            else if (photo.Length > MaxImageBytes)
                TempData["Toast"] = "Fotoğraf en fazla 2 MB olabilir.";
            else
            {
                var folder = Path.Combine(_env.WebRootPath, "images", "users");
                Directory.CreateDirectory(folder);

                // Dosya adı kullanıcıdan alınmaz; rastgele isim verilir
                var fileName = $"{Guid.NewGuid():N}{ext}";
                await using (var stream = System.IO.File.Create(Path.Combine(folder, fileName)))
                    await photo.CopyToAsync(stream);

                DeleteOldPhoto(user.ImageUrl);
                user.ImageUrl = $"/images/users/{fileName}";
                await _userManager.UpdateAsync(user);
                TempData["Toast"] = "Profil fotoğrafınız güncellendi.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemovePhoto()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            DeleteOldPhoto(user.ImageUrl);
            user.ImageUrl = null;
            await _userManager.UpdateAsync(user);

            TempData["Toast"] = "Profil fotoğrafı kaldırıldı.";
            return RedirectToAction(nameof(Index));
        }

        // Şifre değiştirme
        [HttpPost]
        public async Task<IActionResult> ChangePassword([Bind(Prefix = "Password")] ChangePasswordViewModel password)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (ModelState.IsValid)
            {
                var result = await _userManager.ChangePasswordAsync(user, password.CurrentPassword, password.NewPassword);
                if (result.Succeeded)
                {
                    // Güvenlik damgası değiştiği için oturumu yenile
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["Toast"] = "Şifreniz başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                    ModelState.AddModelError("Password", error.Description);
            }

            var model = await BuildModelAsync(user);
            ViewData["PasswordOpen"] = true;
            return View(nameof(Index), model);
        }

        private async Task<ProfileViewModel> BuildModelAsync(AppUser user)
        {
            var email = user.Email ?? "";
            var messages = _context.Messages.AsNoTracking();

            return new ProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? "",
                Email = email,
                EmailConfirmed = user.EmailConfirmed,
                ImageUrl = user.ImageUrl,
                InboxCount = await messages.Inbox(email).CountAsync(),
                SentCount = await messages.Sent(email).CountAsync(),
                UnreadCount = await messages.Inbox(email).CountAsync(m => !m.IsRead),
                StarredCount = await messages.Starred(email).CountAsync(),
                DraftCount = await messages.Drafts(email).CountAsync(),
                Edit = new ProfileEditViewModel
                {
                    Name = user.Name ?? "",
                    Surname = user.Surname ?? "",
                    Description = user.Description
                }
            };
        }

        private void DeleteOldPhoto(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.StartsWith("/images/users/")) return;
            var path = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }
    }
}
