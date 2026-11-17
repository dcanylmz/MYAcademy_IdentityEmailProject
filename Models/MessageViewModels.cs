using System.ComponentModel.DataAnnotations;
using MYAcademy_IdentityEmailProject.Entities;

namespace MYAcademy_IdentityEmailProject.Models
{
    /// <summary>Gelen, Gönderilen, Yıldızlı, Taslaklar, Çöp Kutusu — hepsi aynı liste görünümünü kullanır.</summary>
    public class MessageListViewModel
    {
        public string Box { get; set; } = MessageBoxes.Inbox;
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = "bi-inbox";
        public string EmptyTitle { get; set; } = string.Empty;
        public string EmptyText { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public List<MessageListItem> Items { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
    }

    public class MessageListItem
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string CounterpartEmail { get; set; } = string.Empty;   // gelen kutusunda gönderen, gönderilenlerde alıcı
        public string CounterpartName { get; set; } = string.Empty;
        public string? CounterpartImage { get; set; }
        public bool IsOutgoing { get; set; }                          // kullanıcı bu mesajın göndereni mi?
        public string Subject { get; set; } = string.Empty;
        public string Snippet { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public bool IsUnread { get; set; }
        public bool IsStarred { get; set; }
        public bool IsDraft { get; set; }
        public bool IsExternal { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryColor { get; set; }
    }

    public class MessageDetailViewModel
    {
        public Message Message { get; set; } = null!;
        public string Box { get; set; } = MessageBoxes.Inbox;
        public string BoxTitle { get; set; } = "Gelen Kutusu";
        public string SenderName { get; set; } = string.Empty;
        public string? SenderImage { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public bool IsOutgoing { get; set; }
        public bool IsStarred { get; set; }
        public bool IsInTrash { get; set; }
        public string SafeBody { get; set; } = string.Empty;
        public int Position { get; set; }
        public int Total { get; set; }
        public int? PrevId { get; set; }
        public int? NextId { get; set; }
        public List<MessageListItem> NextUp { get; set; } = new();
    }

    public class ComposeMessageViewModel
    {
        public int? MessageID { get; set; }          // taslak düzenleniyorsa dolu
        public int? ParentMessageID { get; set; }    // yanıt / iletme
        public string Mode { get; set; } = "new";    // new | reply | forward | draft

        [Required(ErrorMessage = "Alıcı e-posta adresini girin.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin (ör. ad@alanadi.com).")]
        [StringLength(256)]
        public string? ReceiverEmail { get; set; }

        [Required(ErrorMessage = "Lütfen bir kategori seçin.")]
        public int? CategoryID { get; set; }

        [Required(ErrorMessage = "Konu alanı boş bırakılamaz.")]
        [StringLength(200, ErrorMessage = "Konu en fazla 200 karakter olabilir.")]
        public string? MessageSubject { get; set; }

        [Required(ErrorMessage = "Mesaj içeriği boş olamaz.")]
        public string? MessageDetail { get; set; }
    }

    public static class MessageBoxes
    {
        public const string Inbox = "inbox";
        public const string Sent = "sent";
        public const string Starred = "starred";
        public const string Drafts = "drafts";
        public const string Trash = "trash";

        public static readonly string[] All = { Inbox, Sent, Starred, Drafts, Trash };

        public static string Title(string box) => box switch
        {
            Sent => "Gönderilenler",
            Starred => "Yıldızlı Mesajlar",
            Drafts => "Taslaklar",
            Trash => "Çöp Kutusu",
            _ => "Gelen Kutusu"
        };
    }
}
