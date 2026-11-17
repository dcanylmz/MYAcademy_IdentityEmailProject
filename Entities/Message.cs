namespace MYAcademy_IdentityEmailProject.Entities
{
    public class Message
    {
        public int MessageID { get; set; }
        public string MessageSubject { get; set; } = string.Empty;
        public string MessageDetail { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string ReceiverEmail { get; set; } = string.Empty;
        public DateTime SendDate { get; set; }

        // Alıcının okuma durumu
        public bool IsRead { get; set; }

        // Eski alan (mig1). Artık kullanılmıyor; yıldız durumu her taraf için ayrı tutuluyor.
        public bool IsFavourite { get; set; }

        // Taslaklar yalnızca gönderenin "Taslaklar" kutusunda görünür
        public bool IsDraft { get; set; }

        // Alıcı uygulamada kayıtlı değilse mesaj SMTP ile gerçek adrese gönderilir
        public bool IsExternal { get; set; }

        // Aynı satırı gönderen ve alıcı paylaştığı için durumlar iki taraf için ayrı tutulur.
        // Böylece alıcının sildiği mesaj gönderenin "Gönderilenler" kutusundan kaybolmaz.
        public bool IsStarredBySender { get; set; }
        public bool IsStarredByReceiver { get; set; }
        public bool IsDeletedBySender { get; set; }              // çöp kutusunda
        public bool IsDeletedByReceiver { get; set; }
        public bool IsPermanentlyDeletedBySender { get; set; }   // çöp kutusundan da silindi
        public bool IsPermanentlyDeletedByReceiver { get; set; }

        // Yanıt / iletme zinciri
        public int? ParentMessageID { get; set; }

        public int? CategoryID { get; set; }
        public Category? Category { get; set; }
    }
}
