using System.ComponentModel.DataAnnotations;

namespace MYAcademy_IdentityEmailProject.Models
{
    public class ConfirmMailViewModel
    {
        [Required(ErrorMessage = "Email adresi gerekli.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi girin.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "6 haneli doğrulama kodunu girin.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Doğrulama kodu 6 haneli olmalıdır.")]
        public string Code { get; set; } = string.Empty;
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email adresinizi girin.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi girin.")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifrenizi girin.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Şifreler eşleşmiyor.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifrenizi girin.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifrenizi girin.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Şifreler eşleşmiyor.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
