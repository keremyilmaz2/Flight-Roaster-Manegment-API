using FlightRosterAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FlightRosterAPI.Models.DTOs.User
{
    // Register DTO
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email alanı zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rol seçimi zorunludur")]
        public UserRole Role { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Nationality { get; set; }
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Doğum tarihi zorunludur")]
        public DateTime DateOfBirth { get; set; }
    }

    // Update DTO
    public class UpdateUserDto
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? Nationality { get; set; }
        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public bool? IsActive { get; set; }
    }

    // Response DTO
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string RoleName => Role.ToString();
        public string? PhoneNumber { get; set; }
        public string? Nationality { get; set; }
        public string? Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age => CalculateAge();
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        private int CalculateAge()
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    // Login DTO
    public class LoginDto
    {
        [Required(ErrorMessage = "Email veya kullanıcı adı zorunludur")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur")]
        public string Password { get; set; } = string.Empty;
    }

    // Login Response DTO
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserResponseDto User { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }

    // Change Password DTO
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mevcut şifre zorunludur")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre zorunludur")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı zorunludur")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}