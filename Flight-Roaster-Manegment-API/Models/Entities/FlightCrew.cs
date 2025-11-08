using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightRosterAPI.Models.Entities
{
    public class FlightCrew
    {
        [Key]
        public int FlightCrewId { get; set; }

        [Required]
        [ForeignKey(nameof(Flight))]
        public int FlightId { get; set; }

        [Required]
        [ForeignKey(nameof(Pilot))]
        public int PilotId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty; // Captain, First Officer, etc.

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Flight Flight { get; set; } = null!;
        public virtual Pilot Pilot { get; set; } = null!;
    }
}