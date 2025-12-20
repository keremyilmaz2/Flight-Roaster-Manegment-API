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

    // Models/DTOs/Seat/BookSeatForSelfDto.cs
    public class BookSeatForSelfDto
    {
        public bool IsInfantSeat { get; set; } = false;
        public int? ParentPassengerId { get; set; }
    }

    // Response DTO
    public class SeatResponseDto
    {
        public int SeatId { get; set; }
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        // ✅ Detaylı Flight Bilgileri Eklendi
        public FlightInfoDto? FlightInfo { get; set; }
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

    // Models/DTOs/Seat/FlightInfoDto.cs
    public class FlightInfoDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;

        // Kalkış Bilgileri
        public string DepartureCountry { get; set; } = string.Empty;
        public string DepartureCity { get; set; } = string.Empty;
        public string DepartureAirport { get; set; } = string.Empty;
        public string DepartureAirportCode { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }

        // Varış Bilgileri
        public string ArrivalCountry { get; set; } = string.Empty;
        public string ArrivalCity { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
        public string ArrivalAirportCode { get; set; } = string.Empty;
        public DateTime ArrivalTime { get; set; }

        // Uçuş Detayları
        public int DurationMinutes { get; set; }
        public string DurationText { get; set; } = string.Empty;
        public double DistanceKm { get; set; }

        // Aircraft Bilgileri
        public string? AircraftType { get; set; }
        public string? AircraftRegistration { get; set; }

        // Code Share
        public string? CodeShareFlightNumber { get; set; }
        public string? CodeShareAirline { get; set; }

        // Computed Properties
        public string RouteText { get; set; } = string.Empty; // "IST → JFK"
        public bool IsDeparted { get; set; }
        public bool IsCompleted { get; set; }
    }
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