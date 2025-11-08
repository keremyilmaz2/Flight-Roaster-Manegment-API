using Flight_Roaster_Manegment_API.Models.Enums;
using FlightRosterAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FlightRosterAPI.Models.DTOs.Seat
{
    // Create DTO
    public class CreateSeatDto
    {
        [Required(ErrorMessage = "Uçuş ID zorunludur")]
        public int FlightId { get; set; }

        [Required(ErrorMessage = "Koltuk numarası zorunludur")]
        [MaxLength(10)]
        public string SeatNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Koltuk sınıfı zorunludur")]
        public SeatClass SeatClass { get; set; }
    }

    // Book Seat DTO (Yolcu için koltuk rezervasyonu)
    public class BookSeatDto
    {
        [Required(ErrorMessage = "Yolcu ID zorunludur")]
        public int PassengerId { get; set; }

        public bool IsInfantSeat { get; set; } = false;

        public int? ParentPassengerId { get; set; } // Bebek için ebeveyn ID
    }

    // Update DTO
    public class UpdateSeatDto
    {
        [MaxLength(10)]
        public string? SeatNumber { get; set; }

        public SeatClass? SeatClass { get; set; }

        public int? PassengerId { get; set; }

        public bool? IsInfantSeat { get; set; }

        public int? ParentPassengerId { get; set; }

        public bool? IsOccupied { get; set; }
    }

    // Response DTO
    public class SeatResponseDto
    {
        public int SeatId { get; set; }
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public int? PassengerId { get; set; }
        public string? PassengerName { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public SeatClass SeatClass { get; set; }
        public string SeatClassName => SeatClass.ToString();
        public bool IsInfantSeat { get; set; }
        public int? ParentPassengerId { get; set; }
        public string? ParentPassengerName { get; set; }
        public bool IsOccupied { get; set; }
        public DateTime? BookedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Seat Map Response (Uçak koltuk planı için)
    public class SeatMapResponseDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string AircraftType { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public int OccupiedSeats { get; set; }
        public List<SeatResponseDto> Seats { get; set; } = new();
    }
}