using FlightRosterAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightRosterAPI.Models.Entities
{
    public class Seat
    {
        [Key]
        public int SeatId { get; set; }

        [Required]
        [ForeignKey(nameof(Flight))]
        public int FlightId { get; set; }

        [ForeignKey(nameof(Passenger))]
        public int? PassengerId { get; set; }

        [Required]
        [MaxLength(10)]
        public string SeatNumber { get; set; } = string.Empty; // Örn: 12A, 5C

        [Required]
        public SeatClass SeatClass { get; set; }

        public bool IsInfantSeat { get; set; } = false; // Bebek için rezerve mi?

        [ForeignKey(nameof(ParentPassenger))]
        public int? ParentPassengerId { get; set; } // Bebek için ebeveyn

        public bool IsOccupied { get; set; } = false;

        public DateTime? BookedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual Flight Flight { get; set; } = null!;
        public virtual Passenger? Passenger { get; set; }
        public virtual Passenger? ParentPassenger { get; set; }
    }
}