using FlightRosterAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightRosterAPI.Models.Entities
{
    public class CabinCrew
    {
        [Key]
        public int CabinCrewId { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        public CabinCrewType CrewType { get; set; }

        [Required]
        public CabinCrewSeniority Seniority { get; set; }

        [Required]
        [MaxLength(500)]
        public string QualifiedAircraftTypes { get; set; } = string.Empty; // Virgülle ayrılmış

        [MaxLength(1000)]
        public string? Recipes { get; set; } // Sadece Chef için, virgülle ayrılmış tarifeler

        [MaxLength(100)]
        public string? Languages { get; set; } // Konuşulan diller, virgülle ayrılmış

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<FlightCabinCrew> FlightCabinCrews { get; set; } = new List<FlightCabinCrew>();
    }
}