using System.ComponentModel.DataAnnotations;

namespace FlightRosterAPI.Models.DTOs.Aircraft
{
    // Create DTO
    public class CreateAircraftDto
    {
        [Required(ErrorMessage = "Uçak tipi zorunludur")]
        [MaxLength(50)]
        public string AircraftType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kayıt numarası zorunludur")]
        [MaxLength(20)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000, ErrorMessage = "Toplam koltuk sayısı 1-1000 arasında olmalıdır")]
        public int TotalSeats { get; set; }

        [Required]
        [Range(0, 500, ErrorMessage = "Business sınıfı koltuk sayısı 0-500 arasında olmalıdır")]
        public int BusinessClassSeats { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Economy sınıfı koltuk sayısı 0-1000 arasında olmalıdır")]
        public int EconomyClassSeats { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Minimum mürettebat sayısı 1-20 arasında olmalıdır")]
        public int MinCrewRequired { get; set; }

        [Required]
        [Range(1, 50, ErrorMessage = "Maksimum mürettebat kapasitesi 1-50 arasında olmalıdır")]
        public int MaxCrewCapacity { get; set; }

        [Required]
        [Range(1, 50, ErrorMessage = "Minimum kabin ekibi sayısı 1-50 arasında olmalıdır")]
        public int MinCabinCrewRequired { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Maksimum kabin ekibi kapasitesi 1-100 arasında olmalıdır")]
        public int MaxCabinCrewCapacity { get; set; }

        [Required]
        [Range(100, 20000, ErrorMessage = "Maksimum menzil 100-20000 km arasında olmalıdır")]
        public double MaxRangeKm { get; set; }
    }

    // Update DTO
    public class UpdateAircraftDto
    {
        [MaxLength(50)]
        public string? AircraftType { get; set; }

        [MaxLength(20)]
        public string? RegistrationNumber { get; set; }

        [Range(1, 1000, ErrorMessage = "Toplam koltuk sayısı 1-1000 arasında olmalıdır")]
        public int? TotalSeats { get; set; }

        [Range(0, 500, ErrorMessage = "Business sınıfı koltuk sayısı 0-500 arasında olmalıdır")]
        public int? BusinessClassSeats { get; set; }

        [Range(0, 1000, ErrorMessage = "Economy sınıfı koltuk sayısı 0-1000 arasında olmalıdır")]
        public int? EconomyClassSeats { get; set; }

        [Range(1, 20, ErrorMessage = "Minimum mürettebat sayısı 1-20 arasında olmalıdır")]
        public int? MinCrewRequired { get; set; }

        [Range(1, 50, ErrorMessage = "Maksimum mürettebat kapasitesi 1-50 arasında olmalıdır")]
        public int? MaxCrewCapacity { get; set; }

        [Range(1, 50, ErrorMessage = "Minimum kabin ekibi sayısı 1-50 arasında olmalıdır")]
        public int? MinCabinCrewRequired { get; set; }

        [Range(1, 100, ErrorMessage = "Maksimum kabin ekibi kapasitesi 1-100 arasında olmalıdır")]
        public int? MaxCabinCrewCapacity { get; set; }

        [Range(100, 20000, ErrorMessage = "Maksimum menzil 100-20000 km arasında olmalıdır")]
        public double? MaxRangeKm { get; set; }

        public bool? IsActive { get; set; }
    }

    // Response DTO
    public class AircraftResponseDto
    {
        public int AircraftId { get; set; }
        public string AircraftType { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int BusinessClassSeats { get; set; }
        public int EconomyClassSeats { get; set; }
        public int MinCrewRequired { get; set; }
        public int MaxCrewCapacity { get; set; }
        public int MinCabinCrewRequired { get; set; }
        public int MaxCabinCrewCapacity { get; set; }
        public double MaxRangeKm { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}