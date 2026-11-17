using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Helpers;
using MYAcademy_IdentityEmailProject.Models;
using MYAcademy_IdentityEmailProject.Services;

namespace MYAcademy_IdentityEmailProject.ViewComponents.MessageLayoutViewComponents
{
    // Navbar: oturum açan kullanıcının bilgileri + okunmamış mesaj bildirimleri
    public class _MessageLayoutHeaderComponentPartial : ViewComponent
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public _MessageLayoutHeaderComponentPartial(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new HeaderViewModel();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user?.Email == null) return View(model);

            model.FullName = UiHelpers.FullName(user.Name, user.Surname, user.UserName);
            model.FirstName = string.IsNullOrWhiteSpace(user.Name) ? user.UserName ?? "" : user.Name;
            model.Email = user.Email;
            model.UserName = user.UserName ?? "";
            model.ImageUrl = user.ImageUrl;

            var unread = _context.Messages.AsNoTracking().Inbox(user.Email).Where(m => !m.IsRead);
            model.UnreadCount = await unread.CountAsync();

            var latest = await unread.OrderByDescending(m => m.SendDate).Take(5).ToListAsync();
            var emails = latest.Select(m => m.SenderEmail.ToUpperInvariant()).Distinct().ToList();
            var senders = await _userManager.Users.AsNoTracking()
                .Where(u => u.NormalizedEmail != null && emails.Contains(u.NormalizedEmail)).ToListAsync();

            model.Notifications = latest.Select(m =>
            {
                var s = senders.FirstOrDefault(u => string.Equals(u.Email, m.SenderEmail, StringComparison.OrdinalIgnoreCase));
                return new MessageListItem
                {
                    Id = m.MessageID,
                    Url = Url.Action("Detail", "Message", new { id = m.MessageID })!,
                    CounterpartEmail = m.SenderEmail,
                    CounterpartName = s != null ? UiHelpers.FullName(s.Name, s.Surname, s.Email) : m.SenderEmail,
                    CounterpartImage = s?.ImageUrl,
                    Subject = string.IsNullOrWhiteSpace(m.MessageSubject) ? "(Konu yok)" : m.MessageSubject,
                    Date = m.SendDate
                };
            }).ToList();

            return View(model);
        }
    }
}
