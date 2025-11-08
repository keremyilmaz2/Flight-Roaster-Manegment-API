using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightRosterAPI.Models.Entities
{
    public class FlightCabinCrew
    {
        [Key]
        public int FlightCabinCrewId { get; set; }

        [Required]
        [ForeignKey(nameof(Flight))]
        public int FlightId { get; set; }

        [Required]
        [ForeignKey(nameof(CabinCrew))]
        public int CabinCrewId { get; set; }

        [MaxLength(200)]
        public string? AssignedRecipe { get; set; } // Sadece Chef için seçilen tarif

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Flight Flight { get; set; } = null!;
        public virtual CabinCrew CabinCrew { get; set; } = null!;
    }
}