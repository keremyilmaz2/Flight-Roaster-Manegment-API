using System.ComponentModel.DataAnnotations;

namespace FlightRosterAPI.Models.DTOs.Roster
{
    // Flight Crew Assignment DTOs
    public class AssignFlightCrewDto
    {
        [Required(ErrorMessage = "Uçuş ID zorunludur")]
        public int FlightId { get; set; }

        [Required(ErrorMessage = "Pilot ID zorunludur")]
        public int PilotId { get; set; }

        [Required(ErrorMessage = "Rol zorunludur")]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty; // Captain, First Officer
    }

    public class FlightCrewResponseDto
    {
        public int FlightCrewId { get; set; }
        public int FlightId { get; set; }
        public int PilotId { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public bool IsActive { get; set; }
    }

    // Cabin Crew Assignment DTOs
    public class AssignCabinCrewDto
    {
        [Required(ErrorMessage = "Uçuş ID zorunludur")]
        public int FlightId { get; set; }

        [Required(ErrorMessage = "Kabin ekibi ID zorunludur")]
        public int CabinCrewId { get; set; }

        [MaxLength(200)]
        public string? AssignedRecipe { get; set; } // Chef için seçilen tarif
    }

    public class FlightCabinCrewResponseDto
    {
        public int FlightCabinCrewId { get; set; }
        public int FlightId { get; set; }
        public int CabinCrewId { get; set; }
        public string? AssignedRecipe { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsActive { get; set; }
    }

    // Auto Assignment DTOs
    public class AutoAssignCrewDto
    {
        [Required(ErrorMessage = "Uçuş ID zorunludur")]
        public int FlightId { get; set; }

        public bool AssignPilots { get; set; } = true;
        public bool AssignCabinCrew { get; set; } = true;
    }

    // Complete Roster Response
    public class FlightRosterResponseDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string AircraftType { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;

        public FlightCrewSummary CrewSummary { get; set; } = new();
        public List<RosterPilotDto> Pilots { get; set; } = new();
        public List<RosterCabinCrewDto> CabinCrew { get; set; } = new();
        public List<RosterPassengerDto> Passengers { get; set; } = new();
    }

    public class FlightCrewSummary
    {
        public int TotalPilots { get; set; }
        public int SeniorPilots { get; set; }
        public int JuniorPilots { get; set; }
        public int TraineePilots { get; set; }
        public int TotalCabinCrew { get; set; }
        public int SeniorCabinCrew { get; set; }
        public int JuniorCabinCrew { get; set; }
        public int Chefs { get; set; }
        public int TotalPassengers { get; set; }
        public int BusinessPassengers { get; set; }
        public int EconomyPassengers { get; set; }
        public int Infants { get; set; }
    }

    public class RosterPilotDto
    {
        public int PilotId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Seniority { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public int TotalFlightHours { get; set; }
    }

    public class RosterCabinCrewDto
    {
        public int CabinCrewId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string CrewType { get; set; } = string.Empty;
        public string Seniority { get; set; } = string.Empty;
        public string? AssignedRecipe { get; set; }
        public List<string>? Languages { get; set; }
    }

    public class RosterPassengerDto
    {
        public int PassengerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? Nationality { get; set; }
        public string? Gender { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string SeatClass { get; set; } = string.Empty;
        public bool IsInfant { get; set; }
        public string? ParentName { get; set; }
    }
}