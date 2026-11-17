using System.ComponentModel.DataAnnotations;

namespace MYAcademy_IdentityEmailProject.Models
{
    public class UserLoginViewModel
    {
        // Kullanıcı adı veya e-posta adresi
        [Required(ErrorMessage = "Kullanıcı adı veya email girin.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifrenizi girin.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
