using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightRosterAPI.Models.Entities
{
    public class Flight
    {
        [Key]
        public int FlightId { get; set; }

        [Required]
        [MaxLength(10)]
        public string FlightNumber { get; set; } = string.Empty; // Örn: AB1234

        [Required]
        [ForeignKey(nameof(Aircraft))]
        public int AircraftId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DepartureCountry { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string DepartureCity { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string DepartureAirport { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string DepartureAirportCode { get; set; } = string.Empty; // Örn: IST

        [Required]
        [MaxLength(100)]
        public string ArrivalCountry { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ArrivalCity { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ArrivalAirport { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string ArrivalAirportCode { get; set; } = string.Empty; // Örn: JFK

        public DateTime DepartureTime { get; set; }

        public DateTime ArrivalTime { get; set; }

        public int DurationMinutes { get; set; }

        public double DistanceKm { get; set; }

        [MaxLength(10)]
        public string? CodeShareFlightNumber { get; set; } // Ortak uçuş numarası

        [MaxLength(100)]
        public string? CodeShareAirline { get; set; } // Ortak havayolu

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Aircraft Aircraft { get; set; } = null!;
        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public virtual ICollection<FlightCrew> FlightCrews { get; set; } = new List<FlightCrew>();
        public virtual ICollection<FlightCabinCrew> FlightCabinCrews { get; set; } = new List<FlightCabinCrew>();
    }
}