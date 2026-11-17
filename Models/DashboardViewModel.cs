namespace MYAcademy_IdentityEmailProject.Models
{
    public class DashboardViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public int Received { get; set; }
        public int Sent { get; set; }
        public int Unread { get; set; }
        public int Starred { get; set; }
        public int StarredUnread { get; set; }
        public int Drafts { get; set; }
        public int Trash { get; set; }
        public int TodayReceived { get; set; }
        public int WeekSent { get; set; }

        public List<MessageListItem> RecentMessages { get; set; } = new();
        public List<CategorySlice> Distribution { get; set; } = new();
        public List<ContactStat> TopContacts { get; set; } = new();

        // Son 90 günün günlük gelen / gönderilen sayıları (grafikte 7-30-90 gün seçilir)
        public List<string> TrafficLabels { get; set; } = new();
        public List<int> TrafficIncoming { get; set; } = new();
        public List<int> TrafficSent { get; set; } = new();
    }

    public record CategorySlice(string Label, string Color, int Value);

    public record ContactStat(string Name, string Email, string? ImageUrl, int Total);
}
