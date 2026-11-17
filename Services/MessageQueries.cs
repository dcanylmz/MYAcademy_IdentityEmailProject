using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;

namespace MYAcademy_IdentityEmailProject.Services
{
    /// <summary>
    /// Her kutu için "bu kullanıcı hangi mesajları görebilir" sorguları.
    /// Tüm controller sorguları buradan geçtiği için kullanıcı başkasının mesajına ulaşamaz.
    /// </summary>
    public static class MessageQueries
    {
        public static IQueryable<Message> Inbox(this IQueryable<Message> q, string email) =>
            q.Where(m => m.ReceiverEmail == email && !m.IsDraft
                         && !m.IsDeletedByReceiver && !m.IsPermanentlyDeletedByReceiver);

        public static IQueryable<Message> Sent(this IQueryable<Message> q, string email) =>
            q.Where(m => m.SenderEmail == email && !m.IsDraft
                         && !m.IsDeletedBySender && !m.IsPermanentlyDeletedBySender);

        public static IQueryable<Message> Drafts(this IQueryable<Message> q, string email) =>
            q.Where(m => m.SenderEmail == email && m.IsDraft && !m.IsPermanentlyDeletedBySender);

        public static IQueryable<Message> Starred(this IQueryable<Message> q, string email) =>
            q.Where(m => !m.IsDraft && (
                (m.ReceiverEmail == email && m.IsStarredByReceiver && !m.IsDeletedByReceiver && !m.IsPermanentlyDeletedByReceiver) ||
                (m.SenderEmail == email && m.IsStarredBySender && !m.IsDeletedBySender && !m.IsPermanentlyDeletedBySender)));

        public static IQueryable<Message> Trash(this IQueryable<Message> q, string email) =>
            q.Where(m => !m.IsDraft && (
                (m.ReceiverEmail == email && m.IsDeletedByReceiver && !m.IsPermanentlyDeletedByReceiver) ||
                (m.SenderEmail == email && m.IsDeletedBySender && !m.IsPermanentlyDeletedBySender)));

        /// <summary>Kullanıcının herhangi bir kutusunda (çöp dahil) görünen tüm mesajlar.</summary>
        public static IQueryable<Message> Accessible(this IQueryable<Message> q, string email) =>
            q.Where(m =>
                (m.SenderEmail == email && !m.IsPermanentlyDeletedBySender) ||
                (m.ReceiverEmail == email && !m.IsDraft && !m.IsPermanentlyDeletedByReceiver));

        public static IQueryable<Message> ForBox(this IQueryable<Message> q, string box, string email) => box switch
        {
            MessageBoxes.Sent => q.Sent(email),
            MessageBoxes.Starred => q.Starred(email),
            MessageBoxes.Drafts => q.Drafts(email),
            MessageBoxes.Trash => q.Trash(email),
            _ => q.Inbox(email)
        };

        // ---------- Tek mesaj üzerinde kullanıcıya göre durum ----------

        public static bool IsSender(this Message m, string email) =>
            string.Equals(m.SenderEmail, email, StringComparison.OrdinalIgnoreCase);

        public static bool IsReceiver(this Message m, string email) =>
            !m.IsDraft && string.Equals(m.ReceiverEmail, email, StringComparison.OrdinalIgnoreCase);

        public static bool IsStarredFor(this Message m, string email) =>
            (m.IsReceiver(email) && m.IsStarredByReceiver) || (m.IsSender(email) && m.IsStarredBySender);

        public static bool IsInTrashFor(this Message m, string email) =>
            (m.IsReceiver(email) && m.IsDeletedByReceiver) || (m.IsSender(email) && m.IsDeletedBySender);

        public static bool IsUnreadFor(this Message m, string email) => m.IsReceiver(email) && !m.IsRead;

        public static void SetStar(this Message m, string email, bool on)
        {
            if (m.IsReceiver(email)) m.IsStarredByReceiver = on;
            if (m.IsSender(email)) m.IsStarredBySender = on;
        }

        public static void SetTrash(this Message m, string email, bool on)
        {
            if (m.IsReceiver(email)) m.IsDeletedByReceiver = on;
            if (m.IsSender(email)) m.IsDeletedBySender = on;
        }

        public static void DeletePermanently(this Message m, string email)
        {
            if (m.IsReceiver(email)) m.IsPermanentlyDeletedByReceiver = true;
            if (m.IsSender(email)) m.IsPermanentlyDeletedBySender = true;
        }

        /// <summary>İki taraf da kalıcı olarak sildiyse satır veritabanından kaldırılabilir.</summary>
        public static bool CanBeRemoved(this Message m) =>
            m.IsDraft ? m.IsPermanentlyDeletedBySender
                      : m.IsPermanentlyDeletedBySender && (m.IsExternal || m.IsPermanentlyDeletedByReceiver);
    }
}
