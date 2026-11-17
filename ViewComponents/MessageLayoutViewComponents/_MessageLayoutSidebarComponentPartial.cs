using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;
using MYAcademy_IdentityEmailProject.Services;

namespace MYAcademy_IdentityEmailProject.ViewComponents.MessageLayoutViewComponents
{
    // Sidebar: menü rozetleri (okunmamış, taslak, yıldızlı, çöp) ve kategoriler veritabanından gelir
    public class _MessageLayoutSidebarComponentPartial : ViewComponent
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public _MessageLayoutSidebarComponentPartial(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new SidebarViewModel
            {
                Categories = await _context.Categories.AsNoTracking().OrderBy(c => c.CategoryName).ToListAsync()
            };

            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user?.Email != null)
            {
                var email = user.Email;
                var messages = _context.Messages.AsNoTracking();
                model.InboxUnread = await messages.Inbox(email).CountAsync(m => !m.IsRead);
                model.Drafts = await messages.Drafts(email).CountAsync();
                model.Starred = await messages.Starred(email).CountAsync();
                model.Trash = await messages.Trash(email).CountAsync();
                model.TotalMessages = await messages.Inbox(email).CountAsync() + await messages.Sent(email).CountAsync();
            }

            return View(model);
        }
    }
}
