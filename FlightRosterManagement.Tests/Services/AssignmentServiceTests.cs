using FluentAssertions;
using Moq;
using FlightRosterManagement.Tests.Mocks;
using Xunit;

namespace FlightRosterManagement.Tests.Services;

/// <summary>
/// AssignmentService için birim testleri.
/// Ekip görevlendirme ve çakışma kontrolü test edilmektedir.
/// </summary>
public class AssignmentServiceTests : TestBase
{
    private readonly Mock<ICrewAssignmentRepository> _mockAssignmentRepo;
    private readonly Mock<IFlightRepository> _mockFlightRepo;
    private readonly Mock<IPilotRepository> _mockPilotRepo;
    private readonly Mock<ICabinCrewRepository> _mockCabinCrewRepo;
    private readonly AssignmentService _service;

    public AssignmentServiceTests()
    {
        _mockAssignmentRepo = new Mock<ICrewAssignmentRepository>();
        _mockFlightRepo = new Mock<IFlightRepository>();
        _mockPilotRepo = new Mock<IPilotRepository>();
        _mockCabinCrewRepo = new Mock<ICabinCrewRepository>();
        
        _service = new AssignmentService(
            _mockAssignmentRepo.Object,
            _mockFlightRepo.Object,
            _mockPilotRepo.Object,
            _mockCabinCrewRepo.Object
        );
    }

    #region AssignPilotAsync Tests

    [Fact]
    public async Task AssignPilotAsync_WhenValidData_ShouldAssignPilot()
    {
        // Arrange
        var flight = CreateTestFlight(1, "TK001");
        var pilot = CreateTestPilot(1, "Ahmet", "Yılmaz");
        var dto = new AssignCrewDto(FlightId: 1, CrewMemberId: 1, CrewType: "Pilot", Role: "Captain");

        _mockFlightRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(flight);
        _mockPilotRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pilot);
        _mockAssignmentRepo.Setup(r => r.HasConflictAsync(1, "Pilot", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        _mockAssignmentRepo.Setup(r => r.CreateAsync(It.IsAny<CrewAssignment>()))
            .ReturnsAsync((CrewAssignment a) => { a.Id = 1; return a; });

        // Act
        var result = await _service.AssignCrewAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.FlightId.Should().Be(1);
        result.PilotId.Should().Be(1);
    }

    [Fact]
    public async Task AssignPilotAsync_WhenFlightNotFound_ShouldThrowException()
    {
        // Arrange
        var dto = new AssignCrewDto(FlightId: 999, CrewMemberId: 1, CrewType: "Pilot", Role: "Captain");
        _mockFlightRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Flight?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AssignCrewAsync(dto));
    }

    [Fact]
    public async Task AssignPilotAsync_WhenPilotNotFound_ShouldThrowException()
    {
        // Arrange
        var flight = CreateTestFlight(1, "TK001");
        var dto = new AssignCrewDto(FlightId: 1, CrewMemberId: 999, CrewType: "Pilot", Role: "Captain");

        _mockFlightRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(flight);
        _mockPilotRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Pilot?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AssignCrewAsync(dto));
    }

    [Fact]
    public async Task AssignPilotAsync_WhenConflictExists_ShouldThrowException()
    {
        // Arrange
        var flight = CreateTestFlight(1, "TK001");
        var pilot = CreateTestPilot(1, "Ahmet", "Yılmaz");
        var dto = new AssignCrewDto(FlightId: 1, CrewMemberId: 1, CrewType: "Pilot", Role: "Captain");

        _mockFlightRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(flight);
        _mockPilotRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pilot);
        _mockAssignmentRepo.Setup(r => r.HasConflictAsync(1, "Pilot", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true); // Conflict exists

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AssignCrewAsync(dto));
    }

    #endregion

    #region AssignCabinCrewAsync Tests

    [Fact]
    public async Task AssignCabinCrewAsync_WhenValidData_ShouldAssignCrew()
    {
        // Arrange
        var flight = CreateTestFlight(1, "TK001");
        var cabinCrew = CreateTestCabinCrew(1, "Ayşe", "Demir");
        var dto = new AssignCrewDto(FlightId: 1, CrewMemberId: 1, CrewType: "CabinCrew", Role: "Purser");

        _mockFlightRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(flight);
        _mockCabinCrewRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cabinCrew);
        _mockAssignmentRepo.Setup(r => r.HasConflictAsync(1, "CabinCrew", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        _mockAssignmentRepo.Setup(r => r.CreateAsync(It.IsAny<CrewAssignment>()))
            .ReturnsAsync((CrewAssignment a) => { a.Id = 1; return a; });

        // Act
        var result = await _service.AssignCrewAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.CabinCrewId.Should().Be(1);
    }

    #endregion

    #region License Validation Tests

    [Fact]
    public async Task AssignPilotAsync_WhenLicenseExpired_ShouldThrowException()
    {
        // Arrange
        var flight = CreateTestFlight(1, "TK001");
        var pilot = CreateTestPilot(1, "Ahmet", "Yılmaz");
        pilot.LicenseExpiryDate = DateTime.UtcNow.AddDays(-1); // Expired

        var dto = new AssignCrewDto(FlightId: 1, CrewMemberId: 1, CrewType: "Pilot", Role: "Captain");

        _mockFlightRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(flight);
        _mockPilotRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pilot);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AssignCrewAsync(dto));
    }

    [Fact]
    public async Task AssignCabinCrewAsync_WhenCertificationExpired_ShouldThrowException()
    {
        // Arrange
        var flight = CreateTestFlight(1, "TK001");
        var cabinCrew = CreateTestCabinCrew(1, "Ayşe", "Demir");
        cabinCrew.CertificationExpiryDate = DateTime.UtcNow.AddDays(-1); // Expired

        var dto = new AssignCrewDto(FlightId: 1, CrewMemberId: 1, CrewType: "CabinCrew", Role: "Purser");

        _mockFlightRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(flight);
        _mockCabinCrewRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cabinCrew);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AssignCrewAsync(dto));
    }

    #endregion

    #region Rest Time Validation Tests - Equivalence Partitioning

    [Theory]
    [InlineData(7, false)]   // Geçersiz - 7 saat (minimum altı)
    [InlineData(8, true)]    // Geçerli - 8 saat (minimum)
    [InlineData(10, true)]   // Geçerli - 10 saat
    [InlineData(12, true)]   // Geçerli - 12 saat (ideal)
    [InlineData(24, true)]   // Geçerli - 24 saat
    public void ValidateRestTime_ShouldReturnCorrectResult(int hours, bool expectedResult)
    {
        // Act
        var result = _service.ValidateRestTime(hours);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Duty Time Validation Tests - Boundary Value Analysis

    [Theory]
    [InlineData(0, true)]    // Minimum
    [InlineData(6, true)]    // Normal
    [InlineData(12, true)]   // Maksimum
    [InlineData(13, false)]  // Maksimum üstü
    public void ValidateDutyTime_BoundaryValues_ShouldReturnCorrectResult(int hours, bool expectedResult)
    {
        // Act
        var result = _service.ValidateDutyTime(hours);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Role Validation Tests

    [Theory]
    [InlineData("Pilot", "Captain", true)]
    [InlineData("Pilot", "FirstOfficer", true)]
    [InlineData("Pilot", "Purser", false)]
    [InlineData("CabinCrew", "Purser", true)]
    [InlineData("CabinCrew", "SeniorCrew", true)]
    [InlineData("CabinCrew", "JuniorCrew", true)]
    [InlineData("CabinCrew", "Captain", false)]
    public void ValidateCrewRole_ShouldReturnCorrectResult(string crewType, string role, bool expectedResult)
    {
        // Act
        var result = _service.ValidateCrewRole(crewType, role);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Helper Methods

    private static Flight CreateTestFlight(int id, string flightNumber)
    {
        return new Flight
        {
            Id = id,
            FlightNumber = flightNumber,
            DepartureAirport = "IST",
            ArrivalAirport = "LHR",
            DepartureTime = DateTime.UtcNow.AddDays(1),
            ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(4),
            AircraftType = "Boeing 777",
            Status = FlightStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Pilot CreateTestPilot(int id, string firstName, string lastName)
    {
        return new Pilot
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            EmployeeNumber = $"PLT{id:D3}",
            DateOfBirth = new DateTime(1985, 1, 1),
            LicenseNumber = $"LIC{id:D3}",
            LicenseExpiryDate = DateTime.UtcNow.AddYears(2),
            TotalFlightHours = 5000,
            Rank = "Captain",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static CabinCrew CreateTestCabinCrew(int id, string firstName, string lastName)
    {
        return new CabinCrew
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            EmployeeNumber = $"CC{id:D3}",
            DateOfBirth = new DateTime(1990, 1, 1),
            Position = "Purser",
            Languages = new List<string> { "Turkish", "English" },
            CertificationExpiryDate = DateTime.UtcNow.AddYears(1),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}

/// <summary>
/// Test amaçlı AssignmentService implementasyonu.
/// </summary>
public class AssignmentService
{
    private readonly ICrewAssignmentRepository _assignmentRepo;
    private readonly IFlightRepository _flightRepo;
    private readonly IPilotRepository _pilotRepo;
    private readonly ICabinCrewRepository _cabinCrewRepo;

    private static readonly string[] PilotRoles = { "Captain", "FirstOfficer" };
    private static readonly string[] CabinCrewRoles = { "Purser", "SeniorCrew", "JuniorCrew" };

    public AssignmentService(
        ICrewAssignmentRepository assignmentRepo,
        IFlightRepository flightRepo,
        IPilotRepository pilotRepo,
        ICabinCrewRepository cabinCrewRepo)
    {
        _assignmentRepo = assignmentRepo;
        _flightRepo = flightRepo;
        _pilotRepo = pilotRepo;
        _cabinCrewRepo = cabinCrewRepo;
    }

    public async Task<CrewAssignment> AssignCrewAsync(AssignCrewDto dto)
    {
        var flight = await _flightRepo.GetByIdAsync(dto.FlightId);
        if (flight == null)
            throw new KeyNotFoundException($"Flight with id {dto.FlightId} not found");

        if (!ValidateCrewRole(dto.CrewType, dto.Role))
            throw new ArgumentException($"Invalid role {dto.Role} for {dto.CrewType}");

        CrewAssignment assignment;

        if (dto.CrewType == "Pilot")
        {
            var pilot = await _pilotRepo.GetByIdAsync(dto.CrewMemberId);
            if (pilot == null)
                throw new KeyNotFoundException($"Pilot with id {dto.CrewMemberId} not found");

            if (pilot.LicenseExpiryDate <= flight.DepartureTime)
                throw new InvalidOperationException("Pilot license will be expired at flight time");

            var hasConflict = await _assignmentRepo.HasConflictAsync(
                dto.CrewMemberId, "Pilot", flight.DepartureTime.AddHours(-2), flight.ArrivalTime.AddHours(1));
            
            if (hasConflict)
                throw new InvalidOperationException("Pilot has a scheduling conflict");

            assignment = new CrewAssignment
            {
                FlightId = dto.FlightId,
                PilotId = dto.CrewMemberId,
                Role = dto.Role,
                AssignedAt = DateTime.UtcNow
            };
        }
        else
        {
            var cabinCrew = await _cabinCrewRepo.GetByIdAsync(dto.CrewMemberId);
            if (cabinCrew == null)
                throw new KeyNotFoundException($"Cabin crew with id {dto.CrewMemberId} not found");

            if (cabinCrew.CertificationExpiryDate <= flight.DepartureTime)
                throw new InvalidOperationException("Cabin crew certification will be expired at flight time");

            var hasConflict = await _assignmentRepo.HasConflictAsync(
                dto.CrewMemberId, "CabinCrew", flight.DepartureTime.AddHours(-2), flight.ArrivalTime.AddHours(1));
            
            if (hasConflict)
                throw new InvalidOperationException("Cabin crew has a scheduling conflict");

            assignment = new CrewAssignment
            {
                FlightId = dto.FlightId,
                CabinCrewId = dto.CrewMemberId,
                Role = dto.Role,
                AssignedAt = DateTime.UtcNow
            };
        }

        return await _assignmentRepo.CreateAsync(assignment);
    }

    public bool ValidateRestTime(int hours)
    {
        return hours >= 8; // Minimum 8 hours rest required
    }

    public bool ValidateDutyTime(int hours)
    {
        return hours >= 0 && hours <= 12; // Maximum 12 hours duty
    }

    public bool ValidateCrewRole(string crewType, string role)
    {
        return crewType switch
        {
            "Pilot" => PilotRoles.Contains(role),
            "CabinCrew" => CabinCrewRoles.Contains(role),
            _ => false
        };
    }
}
