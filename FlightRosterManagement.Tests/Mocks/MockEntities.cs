namespace FlightRosterManagement.Tests.Mocks;

#region Entities

public class Pilot
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTime LicenseExpiryDate { get; set; }
    public int TotalFlightHours { get; set; }
    public string Rank { get; set; } = string.Empty; // Captain, FirstOfficer
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CabinCrew
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Position { get; set; } = string.Empty; // Purser, SeniorCrew, JuniorCrew
    public List<string> Languages { get; set; } = new();
    public DateTime CertificationExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class Flight
{
    public int Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string DepartureAirport { get; set; } = string.Empty;
    public string ArrivalAirport { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string AircraftType { get; set; } = string.Empty;
    public FlightStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CrewAssignment
{
    public int Id { get; set; }
    public int FlightId { get; set; }
    public int? PilotId { get; set; }
    public int? CabinCrewId { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

public enum FlightStatus
{
    Scheduled,
    Boarding,
    Departed,
    InFlight,
    Landed,
    Cancelled,
    Delayed
}

#endregion

#region DTOs

public record PilotDto(
    int Id,
    string FirstName,
    string LastName,
    string EmployeeNumber,
    DateTime DateOfBirth,
    string LicenseNumber,
    DateTime LicenseExpiryDate,
    int TotalFlightHours,
    string Rank,
    bool IsActive
);

public record CreatePilotDto(
    string FirstName,
    string LastName,
    string EmployeeNumber,
    DateTime DateOfBirth,
    string LicenseNumber,
    DateTime LicenseExpiryDate,
    int TotalFlightHours,
    string Rank
);

public record UpdatePilotDto(
    string FirstName,
    string LastName,
    string LicenseNumber,
    DateTime LicenseExpiryDate,
    int TotalFlightHours,
    string Rank,
    bool IsActive
);

public record CabinCrewDto(
    int Id,
    string FirstName,
    string LastName,
    string EmployeeNumber,
    DateTime DateOfBirth,
    string Position,
    List<string> Languages,
    DateTime CertificationExpiryDate,
    bool IsActive
);

public record CreateCabinCrewDto(
    string FirstName,
    string LastName,
    string EmployeeNumber,
    DateTime DateOfBirth,
    string Position,
    List<string> Languages,
    DateTime CertificationExpiryDate
);

public record FlightDto(
    int Id,
    string FlightNumber,
    string DepartureAirport,
    string ArrivalAirport,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    string AircraftType,
    FlightStatus Status
);

public record CreateFlightDto(
    string FlightNumber,
    string DepartureAirport,
    string ArrivalAirport,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    string AircraftType
);

public record AssignCrewDto(
    int FlightId,
    int CrewMemberId,
    string CrewType, // Pilot or CabinCrew
    string Role
);

public record LoginDto(string Email, string Password);
public record RegisterDto(string Email, string Password, string FullName, string Role);
public record TokenDto(string AccessToken, string RefreshToken, DateTime ExpiresAt);

#endregion

#region Interfaces

public interface IPilotRepository
{
    Task<IEnumerable<Pilot>> GetAllAsync();
    Task<Pilot?> GetByIdAsync(int id);
    Task<Pilot?> GetByEmployeeNumberAsync(string employeeNumber);
    Task<Pilot> CreateAsync(Pilot pilot);
    Task<Pilot> UpdateAsync(Pilot pilot);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface ICabinCrewRepository
{
    Task<IEnumerable<CabinCrew>> GetAllAsync();
    Task<CabinCrew?> GetByIdAsync(int id);
    Task<CabinCrew?> GetByEmployeeNumberAsync(string employeeNumber);
    Task<CabinCrew> CreateAsync(CabinCrew cabinCrew);
    Task<CabinCrew> UpdateAsync(CabinCrew cabinCrew);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IFlightRepository
{
    Task<IEnumerable<Flight>> GetAllAsync();
    Task<Flight?> GetByIdAsync(int id);
    Task<Flight?> GetByFlightNumberAsync(string flightNumber);
    Task<IEnumerable<Flight>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<Flight> CreateAsync(Flight flight);
    Task<Flight> UpdateAsync(Flight flight);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface ICrewAssignmentRepository
{
    Task<IEnumerable<CrewAssignment>> GetByFlightIdAsync(int flightId);
    Task<IEnumerable<CrewAssignment>> GetByPilotIdAsync(int pilotId);
    Task<IEnumerable<CrewAssignment>> GetByCabinCrewIdAsync(int cabinCrewId);
    Task<CrewAssignment> CreateAsync(CrewAssignment assignment);
    Task<bool> DeleteAsync(int id);
    Task<bool> HasConflictAsync(int crewMemberId, string crewType, DateTime start, DateTime end);
}

#endregion
