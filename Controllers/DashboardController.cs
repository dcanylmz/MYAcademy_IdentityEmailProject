using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Helpers;
using MYAcademy_IdentityEmailProject.Models;
using MYAcademy_IdentityEmailProject.Services;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    public class DashboardController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me?.Email == null) return Challenge();
            var email = me.Email;
            var q = _context.Messages.AsNoTracking();

            // Tüm sayılar gerçek verilerden hesaplanır
            var inbox = await q.Inbox(email).Include(m => m.Category).OrderByDescending(m => m.SendDate).ToListAsync();
            var sent = await q.Sent(email).Include(m => m.Category).ToListAsync();
            var starred = await q.Starred(email).ToListAsync();

            var today = DateTime.Today;
            var model = new DashboardViewModel
            {
                FirstName = string.IsNullOrWhiteSpace(me.Name) ? me.UserName ?? "" : me.Name,
                Received = inbox.Count,
                Sent = sent.Count,
                Unread = inbox.Count(m => !m.IsRead),
                Starred = starred.Count,
                StarredUnread = starred.Count(m => m.IsUnreadFor(email)),
                Drafts = await q.Drafts(email).CountAsync(),
                Trash = await q.Trash(email).CountAsync(),
                TodayReceived = inbox.Count(m => m.SendDate.Date == today),
                WeekSent = sent.Count(m => m.SendDate.Date > today.AddDays(-7))
            };

            // Kişi bilgileri (ad, fotoğraf)
            var contactEmails = inbox.Select(m => m.SenderEmail).Concat(sent.Select(m => m.ReceiverEmail))
                .Select(e => e.ToUpperInvariant()).Distinct().ToList();
            var users = (await _userManager.Users.AsNoTracking()
                    .Where(u => u.NormalizedEmail != null && contactEmails.Contains(u.NormalizedEmail)).ToListAsync())
                .Where(u => u.Email != null)
                .GroupBy(u => u.Email!.ToLowerInvariant()).ToDictionary(g => g.Key, g => g.First());
            string NameOf(string mail) => users.TryGetValue(mail.ToLowerInvariant(), out var u) ? UiHelpers.FullName(u.Name, u.Surname, mail) : mail;
            string? ImageOf(string mail) => users.TryGetValue(mail.ToLowerInvariant(), out var u) ? u.ImageUrl : null;

            // Son gelen 5 mesaj
            model.RecentMessages = inbox.Take(5).Select(m => new MessageListItem
            {
                Id = m.MessageID,
                Url = Url.Action("Detail", "Message", new { id = m.MessageID })!,
                CounterpartEmail = m.SenderEmail,
                CounterpartName = NameOf(m.SenderEmail),
                CounterpartImage = ImageOf(m.SenderEmail),
                Subject = string.IsNullOrWhiteSpace(m.MessageSubject) ? "(Konu yok)" : m.MessageSubject,
                Snippet = MessageHtml.Snippet(m.MessageDetail, 80),
                Date = m.SendDate,
                IsUnread = !m.IsRead
            }).ToList();

            // Kategorilere göre dağılım (gelen + gönderilen)
            model.Distribution = inbox.Concat(sent)
                .GroupBy(m => m.Category?.CategoryName ?? "Kategorisiz")
                .Select(g => new CategorySlice(g.Key, g.First().Category?.CategoryColor ?? "#C9CBD6", g.Count()))
                .OrderByDescending(s => s.Value)
                .ToList();

            // Son 90 günlük trafik
            var start = today.AddDays(-89);
            for (var d = start; d <= today; d = d.AddDays(1))
            {
                model.TrafficLabels.Add(d.ToString("d MMM", UiHelpers.Tr));
                model.TrafficIncoming.Add(inbox.Count(m => m.SendDate.Date == d));
                model.TrafficSent.Add(sent.Count(m => m.SendDate.Date == d));
            }

            // En sık iletişim kurulan kişiler (son 90 gün)
            model.TopContacts = inbox.Where(m => m.SendDate >= start).Select(m => m.SenderEmail)
                .Concat(sent.Where(m => m.SendDate >= start).Select(m => m.ReceiverEmail))
                .Where(e => !string.Equals(e, email, StringComparison.OrdinalIgnoreCase))
                .GroupBy(e => e.ToLowerInvariant())
                .Select(g => new ContactStat(NameOf(g.First()), g.First(), ImageOf(g.First()), g.Count()))
                .OrderByDescending(c => c.Total).Take(5).ToList();

            return View(model);
        }
    }
}
