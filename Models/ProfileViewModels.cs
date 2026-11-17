using System.ComponentModel.DataAnnotations;

namespace MYAcademy_IdentityEmailProject.Models
{
    public class ProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public string? ImageUrl { get; set; }

        public int InboxCount { get; set; }
        public int SentCount { get; set; }
        public int UnreadCount { get; set; }
        public int StarredCount { get; set; }
        public int DraftCount { get; set; }

        public ProfileEditViewModel Edit { get; set; } = new();
        public ChangePasswordViewModel Password { get; set; } = new();

        public string FullName => $"{Edit.Name} {Edit.Surname}".Trim();
    }

    public class ProfileEditViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter olmalıdır.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter olmalıdır.")]
        public string Surname { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Hakkımda en fazla 300 karakter olabilir.")]
        public string? Description { get; set; }
    }
}
