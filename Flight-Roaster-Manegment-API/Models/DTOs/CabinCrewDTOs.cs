using FlightRosterAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FlightRosterAPI.Models.DTOs.CabinCrew
{
    // Create DTO
    public class CreateCabinCrewDto
    {
        [Required(ErrorMessage = "Kullanıcı ID zorunludur")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Görev tipi zorunludur")]
        public CabinCrewType CrewType { get; set; }

        [Required(ErrorMessage = "Kıdem seviyesi zorunludur")]
        public CabinCrewSeniority Seniority { get; set; }

        [Required(ErrorMessage = "Yetkili uçak tipleri zorunludur")]
        [MaxLength(500)]
        public string QualifiedAircraftTypes { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Recipes { get; set; } // Chef için tarifeler

        [MaxLength(100)]
        public string? Languages { get; set; } // Konuşulan diller
    }

    // Update DTO
    public class UpdateCabinCrewDto
    {
        public CabinCrewType? CrewType { get; set; }

        public CabinCrewSeniority? Seniority { get; set; }

        [MaxLength(500)]
        public string? QualifiedAircraftTypes { get; set; }

        [MaxLength(1000)]
        public string? Recipes { get; set; }

        [MaxLength(100)]
        public string? Languages { get; set; }

        public bool? IsActive { get; set; }
    }

    // Response DTO
    public class CabinCrewResponseDto
    {
        public int CabinCrewId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public CabinCrewType CrewType { get; set; }
        public string CrewTypeName => CrewType.ToString();
        public CabinCrewSeniority Seniority { get; set; }
        public string SeniorityName => Seniority.ToString();
        public List<string> QualifiedAircraftTypes { get; set; } = new();
        public List<string>? Recipes { get; set; }
        public List<string>? Languages { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}