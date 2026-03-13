using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Models
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Kullanıcı adı boş olamaz!")]
        [MinLength(3, ErrorMessage = "Kullanıcı adı en az 3 karakter olmalı!")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre boş olamaz!")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalı!")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}