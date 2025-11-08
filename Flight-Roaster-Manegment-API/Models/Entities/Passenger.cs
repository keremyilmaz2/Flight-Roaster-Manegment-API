using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightRosterAPI.Models.Entities
{
    public class Passenger
    {
        [Key]
        public int PassengerId { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [MaxLength(50)]
        public string? PassportNumber { get; set; }

        [MaxLength(50)]
        public string? NationalIdNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}