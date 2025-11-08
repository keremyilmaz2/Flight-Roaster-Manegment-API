using System.ComponentModel.DataAnnotations;

namespace FlightRosterAPI.Models.Entities
{
    public class Aircraft
    {
        [Key]
        public int AircraftId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AircraftType { get; set; } = string.Empty; // Örn: Boeing 737, Airbus A320

        [Required]
        [MaxLength(20)]
        public string RegistrationNumber { get; set; } = string.Empty; // Örn: TC-ABC

        public int TotalSeats { get; set; }

        public int BusinessClassSeats { get; set; }

        public int EconomyClassSeats { get; set; }

        public int MinCrewRequired { get; set; } // Minimum mürettebat sayısı

        public int MaxCrewCapacity { get; set; } // Maksimum mürettebat kapasitesi

        public int MinCabinCrewRequired { get; set; }

        public int MaxCabinCrewCapacity { get; set; }

        public double MaxRangeKm { get; set; } // Maksimum menzil (km)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
    }
}