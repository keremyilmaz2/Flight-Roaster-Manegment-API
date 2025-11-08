using FlightRosterAPI.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FlightRosterAPI.Models.Entities
{
    public class User : IdentityUser<int> // int Id kullanıyoruz
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [MaxLength(100)]
        public string? Nationality { get; set; }
        public string? Gender { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Full name helper
        public string FullName => $"{FirstName} {LastName}";
    }
}