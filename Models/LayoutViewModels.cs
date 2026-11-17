using MYAcademy_IdentityEmailProject.Entities;

namespace MYAcademy_IdentityEmailProject.Models
{
    public class SidebarViewModel
    {
        public int InboxUnread { get; set; }
        public int Drafts { get; set; }
        public int Starred { get; set; }
        public int Trash { get; set; }
        public int TotalMessages { get; set; }
        public List<Category> Categories { get; set; } = new();
    }

    public class HeaderViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int UnreadCount { get; set; }
        public List<MessageListItem> Notifications { get; set; } = new();
    }
}
