using System.ComponentModel.DataAnnotations;

namespace FlightRosterAPI.Models.DTOs.Passenger
{
    // Create DTO
    public class CreatePassengerDto
    {
        [Required(ErrorMessage = "Kullanıcı ID zorunludur")]
        public int UserId { get; set; }

        [MaxLength(50)]
        public string? PassportNumber { get; set; }

        [MaxLength(50)]
        public string? NationalIdNumber { get; set; }
    }

    // Update DTO
    public class UpdatePassengerDto
    {
        [MaxLength(50)]
        public string? PassportNumber { get; set; }

        [MaxLength(50)]
        public string? NationalIdNumber { get; set; }

        public bool? IsActive { get; set; }
    }

    // Response DTO
    public class PassengerResponseDto
    {
        public int PassengerId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public string? Nationality { get; set; }
        public string? Gender { get; set; }
        public string? PassportNumber { get; set; }
        public string? NationalIdNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}