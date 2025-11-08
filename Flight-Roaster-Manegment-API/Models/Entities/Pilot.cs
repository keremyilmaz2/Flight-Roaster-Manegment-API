using FlightRosterAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightRosterAPI.Models.Entities
{
    public class Pilot
    {
        [Key]
        public int PilotId { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required]
        public PilotSeniority Seniority { get; set; }

        public double MaxFlightDistanceKm { get; set; }

        [Required]
        [MaxLength(500)]
        public string QualifiedAircraftTypes { get; set; } = string.Empty; // Virgülle ayrılmış: "Boeing 737,Airbus A320"

        public int TotalFlightHours { get; set; }

        public DateTime LicenseExpiryDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<FlightCrew> FlightCrews { get; set; } = new List<FlightCrew>();
    }
}