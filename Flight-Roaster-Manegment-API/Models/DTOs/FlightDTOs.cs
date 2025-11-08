using System.ComponentModel.DataAnnotations;

namespace FlightRosterAPI.Models.DTOs.Flight
{
    // Create DTO
    public class CreateFlightDto
    {
        [Required(ErrorMessage = "Uçuş numarası zorunludur")]
        [MaxLength(10)]
        public string FlightNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uçak seçimi zorunludur")]
        public int AircraftId { get; set; }

        [Required(ErrorMessage = "Kalkış ülkesi zorunludur")]
        [MaxLength(100)]
        public string DepartureCountry { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kalkış şehri zorunludur")]
        [MaxLength(100)]
        public string DepartureCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kalkış havaalanı zorunludur")]
        [MaxLength(100)]
        public string DepartureAirport { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kalkış havaalanı kodu zorunludur")]
        [MaxLength(10)]
        public string DepartureAirportCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Varış ülkesi zorunludur")]
        [MaxLength(100)]
        public string ArrivalCountry { get; set; } = string.Empty;

        [Required(ErrorMessage = "Varış şehri zorunludur")]
        [MaxLength(100)]
        public string ArrivalCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Varış havaalanı zorunludur")]
        [MaxLength(100)]
        public string ArrivalAirport { get; set; } = string.Empty;

        [Required(ErrorMessage = "Varış havaalanı kodu zorunludur")]
        [MaxLength(10)]
        public string ArrivalAirportCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kalkış zamanı zorunludur")]
        public DateTime DepartureTime { get; set; }

        [Required(ErrorMessage = "Varış zamanı zorunludur")]
        public DateTime ArrivalTime { get; set; }

        [Required]
        [Range(1, 2000, ErrorMessage = "Uçuş süresi 1-2000 dakika arasında olmalıdır")]
        public int DurationMinutes { get; set; }

        [Required]
        [Range(1, 20000, ErrorMessage = "Mesafe 1-20000 km arasında olmalıdır")]
        public double DistanceKm { get; set; }

        [MaxLength(10)]
        public string? CodeShareFlightNumber { get; set; }

        [MaxLength(100)]
        public string? CodeShareAirline { get; set; }
    }

    // Update DTO
    public class UpdateFlightDto
    {
        [MaxLength(10)]
        public string? FlightNumber { get; set; }

        public int? AircraftId { get; set; }

        [MaxLength(100)]
        public string? DepartureCountry { get; set; }

        [MaxLength(100)]
        public string? DepartureCity { get; set; }

        [MaxLength(100)]
        public string? DepartureAirport { get; set; }

        [MaxLength(10)]
        public string? DepartureAirportCode { get; set; }

        [MaxLength(100)]
        public string? ArrivalCountry { get; set; }

        [MaxLength(100)]
        public string? ArrivalCity { get; set; }

        [MaxLength(100)]
        public string? ArrivalAirport { get; set; }

        [MaxLength(10)]
        public string? ArrivalAirportCode { get; set; }

        public DateTime? DepartureTime { get; set; }

        public DateTime? ArrivalTime { get; set; }

        [Range(1, 2000, ErrorMessage = "Uçuş süresi 1-2000 dakika arasında olmalıdır")]
        public int? DurationMinutes { get; set; }

        [Range(1, 20000, ErrorMessage = "Mesafe 1-20000 km arasında olmalıdır")]
        public double? DistanceKm { get; set; }

        [MaxLength(10)]
        public string? CodeShareFlightNumber { get; set; }

        [MaxLength(100)]
        public string? CodeShareAirline { get; set; }

        public bool? IsActive { get; set; }
    }

    // Response DTO
    public class FlightResponseDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public int AircraftId { get; set; }
        public string AircraftType { get; set; } = string.Empty;
        public string DepartureCountry { get; set; } = string.Empty;
        public string DepartureCity { get; set; } = string.Empty;
        public string DepartureAirport { get; set; } = string.Empty;
        public string DepartureAirportCode { get; set; } = string.Empty;
        public string ArrivalCountry { get; set; } = string.Empty;
        public string ArrivalCity { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
        public string ArrivalAirportCode { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int DurationMinutes { get; set; }
        public double DistanceKm { get; set; }
        public string? CodeShareFlightNumber { get; set; }
        public string? CodeShareAirline { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    // Detailed Flight Response with Crew and Passengers
    public class FlightDetailResponseDto : FlightResponseDto
    {
        public List<FlightCrewMemberDto> Pilots { get; set; } = new();
        public List<FlightCabinCrewMemberDto> CabinCrew { get; set; } = new();
        public List<FlightPassengerDto> Passengers { get; set; } = new();
        public int TotalSeats { get; set; }
        public int OccupiedSeats { get; set; }
        public int AvailableSeats => TotalSeats - OccupiedSeats;
    }

    public class FlightCrewMemberDto
    {
        public int PilotId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Seniority { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
    }

    public class FlightCabinCrewMemberDto
    {
        public int CabinCrewId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string CrewType { get; set; } = string.Empty;
        public string Seniority { get; set; } = string.Empty;
        public string? AssignedRecipe { get; set; }
    }

    public class FlightPassengerDto
    {
        public int PassengerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? Nationality { get; set; }
        public string? Gender { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string SeatClass { get; set; } = string.Empty;
        public bool IsInfant { get; set; }
        public string? ParentName { get; set; }
    }
}