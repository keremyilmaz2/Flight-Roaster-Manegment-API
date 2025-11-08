using FlightRosterAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FlightRosterAPI.Models.DTOs.Pilot
{
    // Create DTO
    public class CreatePilotDto
    {
        [Required(ErrorMessage = "Kullanıcı ID zorunludur")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Lisans numarası zorunludur")]
        [MaxLength(50)]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kıdem seviyesi zorunludur")]
        public PilotSeniority Seniority { get; set; }

        [Required]
        [Range(100, 20000, ErrorMessage = "Maksimum uçuş mesafesi 100-20000 km arasında olmalıdır")]
        public double MaxFlightDistanceKm { get; set; }

        [Required(ErrorMessage = "Yetkili uçak tipleri zorunludur")]
        [MaxLength(500)]
        public string QualifiedAircraftTypes { get; set; } = string.Empty;

        [Range(0, 50000, ErrorMessage = "Toplam uçuş saati 0-50000 arasında olmalıdır")]
        public int TotalFlightHours { get; set; }

        [Required(ErrorMessage = "Lisans son kullanma tarihi zorunludur")]
        public DateTime LicenseExpiryDate { get; set; }
    }

    // Update DTO
    public class UpdatePilotDto
    {
        [MaxLength(50)]
        public string? LicenseNumber { get; set; }

        public PilotSeniority? Seniority { get; set; }

        [Range(100, 20000, ErrorMessage = "Maksimum uçuş mesafesi 100-20000 km arasında olmalıdır")]
        public double? MaxFlightDistanceKm { get; set; }

        [MaxLength(500)]
        public string? QualifiedAircraftTypes { get; set; }

        [Range(0, 50000, ErrorMessage = "Toplam uçuş saati 0-50000 arasında olmalıdır")]
        public int? TotalFlightHours { get; set; }

        public DateTime? LicenseExpiryDate { get; set; }

        public bool? IsActive { get; set; }
    }

    // Response DTO
    public class PilotResponseDto
    {
        public int PilotId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public PilotSeniority Seniority { get; set; }
        public string SeniorityName => Seniority.ToString();
        public double MaxFlightDistanceKm { get; set; }
        public List<string> QualifiedAircraftTypes { get; set; } = new();
        public int TotalFlightHours { get; set; }
        public DateTime LicenseExpiryDate { get; set; }
        public bool IsLicenseValid => LicenseExpiryDate > DateTime.UtcNow;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}