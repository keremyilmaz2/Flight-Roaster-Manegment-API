using FluentAssertions;
using Moq;
using FlightRosterManagement.Tests.Mocks;
using Xunit;

namespace FlightRosterManagement.Tests.Services;

/// <summary>
/// FlightService için birim testleri.
/// Uçuş yönetimi CRUD işlemleri ve durum geçişleri test edilmektedir.
/// </summary>
public class FlightServiceTests : TestBase
{
    private readonly Mock<IFlightRepository> _mockRepository;
    private readonly FlightService _service;

    public FlightServiceTests()
    {
        _mockRepository = new Mock<IFlightRepository>();
        _service = new FlightService(_mockRepository.Object);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenFlightsExist_ShouldReturnAllFlights()
    {
        // Arrange
        var flights = new List<Flight>
        {
            CreateTestFlight(1, "TK001"),
            CreateTestFlight(2, "TK002"),
            CreateTestFlight(3, "TK003")
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(flights);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenFlightExists_ShouldReturnFlight()
    {
        // Arrange
        var flight = CreateTestFlight(1, "TK001");
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(flight);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.FlightNumber.Should().Be("TK001");
    }

    [Fact]
    public async Task GetByIdAsync_WhenFlightNotExists_ShouldReturnNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Flight?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WhenValidData_ShouldCreateFlight()
    {
        // Arrange
        var createDto = new CreateFlightDto(
            FlightNumber: "TK001",
            DepartureAirport: "IST",
            ArrivalAirport: "LHR",
            DepartureTime: DateTime.UtcNow.AddDays(1),
            ArrivalTime: DateTime.UtcNow.AddDays(1).AddHours(4),
            AircraftType: "Boeing 777"
        );

        _mockRepository.Setup(r => r.GetByFlightNumberAsync("TK001")).ReturnsAsync((Flight?)null);
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Flight>()))
            .ReturnsAsync((Flight f) => { f.Id = 1; return f; });

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.FlightNumber.Should().Be("TK001");
        result.Status.Should().Be(FlightStatus.Scheduled);
    }

    [Fact]
    public async Task CreateAsync_WhenFlightNumberExists_ShouldThrowException()
    {
        // Arrange
        var existingFlight = CreateTestFlight(1, "TK001");
        var createDto = new CreateFlightDto(
            FlightNumber: "TK001",
            DepartureAirport: "IST",
            ArrivalAirport: "LHR",
            DepartureTime: DateTime.UtcNow.AddDays(1),
            ArrivalTime: DateTime.UtcNow.AddDays(1).AddHours(4),
            AircraftType: "Boeing 777"
        );

        _mockRepository.Setup(r => r.GetByFlightNumberAsync("TK001")).ReturnsAsync(existingFlight);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WhenDepartureAfterArrival_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreateFlightDto(
            FlightNumber: "TK001",
            DepartureAirport: "IST",
            ArrivalAirport: "LHR",
            DepartureTime: DateTime.UtcNow.AddDays(1).AddHours(5),
            ArrivalTime: DateTime.UtcNow.AddDays(1),
            AircraftType: "Boeing 777"
        );

        _mockRepository.Setup(r => r.GetByFlightNumberAsync("TK001")).ReturnsAsync((Flight?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WhenDepartureInPast_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreateFlightDto(
            FlightNumber: "TK001",
            DepartureAirport: "IST",
            ArrivalAirport: "LHR",
            DepartureTime: DateTime.UtcNow.AddDays(-1),
            ArrivalTime: DateTime.UtcNow,
            AircraftType: "Boeing 777"
        );

        _mockRepository.Setup(r => r.GetByFlightNumberAsync("TK001")).ReturnsAsync((Flight?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(createDto));
    }

    #endregion

    #region CancelAsync Tests

    [Fact]
    public async Task CancelAsync_WhenFlightScheduled_ShouldCancel()
    {
        // Arrange
        var flight = CreateTestFlight(1, "TK001");
        flight.Status = FlightStatus.Scheduled;
        
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(flight);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Flight>()))
            .ReturnsAsync((Flight f) => f);

        // Act
        var result = await _service.CancelAsync(1);

        // Assert
        result.Status.Should().Be(FlightStatus.Cancelled);
    }

    [Fact]
    public async Task CancelAsync_WhenFlightAlreadyDeparted_ShouldThrowException()
    {
        // Arrange
        var flight = CreateTestFlight(1, "TK001");
        flight.Status = FlightStatus.Departed;
        
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(flight);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelAsync(1));
    }

    [Theory]
    [InlineData(FlightStatus.InFlight)]
    [InlineData(FlightStatus.Landed)]
    [InlineData(FlightStatus.Cancelled)]
    public async Task CancelAsync_WhenFlightInInvalidState_ShouldThrowException(FlightStatus status)
    {
        // Arrange
        var flight = CreateTestFlight(1, "TK001");
        flight.Status = status;
        
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(flight);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelAsync(1));
    }

    #endregion

    #region Status Transition Tests

    [Theory]
    [InlineData(FlightStatus.Scheduled, FlightStatus.Boarding, true)]
    [InlineData(FlightStatus.Scheduled, FlightStatus.Cancelled, true)]
    [InlineData(FlightStatus.Scheduled, FlightStatus.Delayed, true)]
    [InlineData(FlightStatus.Boarding, FlightStatus.Departed, true)]
    [InlineData(FlightStatus.Departed, FlightStatus.InFlight, true)]
    [InlineData(FlightStatus.InFlight, FlightStatus.Landed, true)]
    [InlineData(FlightStatus.Landed, FlightStatus.Scheduled, false)]
    [InlineData(FlightStatus.Cancelled, FlightStatus.Boarding, false)]
    public void ValidateStatusTransition_ShouldReturnCorrectResult(
        FlightStatus from, FlightStatus to, bool expectedResult)
    {
        // Act
        var result = _service.ValidateStatusTransition(from, to);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Flight Duration Tests - Boundary Value Analysis

    [Theory]
    [InlineData(0, false)]
    [InlineData(29, false)]
    [InlineData(30, true)]
    [InlineData(60, true)]
    [InlineData(720, true)]
    [InlineData(1080, true)]
    [InlineData(1081, false)]
    public void ValidateFlightDuration_BoundaryValues_ShouldReturnCorrectResult(int minutes, bool expectedResult)
    {
        // Act
        var result = _service.ValidateFlightDuration(minutes);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Airport Code Validation Tests

    [Theory]
    [InlineData("IST", true)]
    [InlineData("LHR", true)]
    [InlineData("JFK", true)]
    [InlineData("AB", false)]
    [InlineData("ABCD", false)]
    [InlineData("123", false)]
    [InlineData("", false)]
    public void ValidateAirportCode_ShouldReturnCorrectResult(string code, bool expectedResult)
    {
        // Act
        var result = _service.ValidateAirportCode(code);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void ValidateAirportCode_WhenSameAirports_ShouldReturnFalse()
    {
        // Act
        var result = _service.ValidateDifferentAirports("IST", "IST");

        // Assert
        result.Should().BeFalse();
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

    #endregion
}

/// <summary>
/// Test amaçlı FlightService implementasyonu.
/// </summary>
public class FlightService
{
    private readonly IFlightRepository _repository;

    private static readonly Dictionary<FlightStatus, FlightStatus[]> ValidTransitions = new()
    {
        { FlightStatus.Scheduled, new[] { FlightStatus.Boarding, FlightStatus.Cancelled, FlightStatus.Delayed } },
        { FlightStatus.Delayed, new[] { FlightStatus.Boarding, FlightStatus.Cancelled } },
        { FlightStatus.Boarding, new[] { FlightStatus.Departed, FlightStatus.Cancelled } },
        { FlightStatus.Departed, new[] { FlightStatus.InFlight } },
        { FlightStatus.InFlight, new[] { FlightStatus.Landed } },
        { FlightStatus.Landed, Array.Empty<FlightStatus>() },
        { FlightStatus.Cancelled, Array.Empty<FlightStatus>() }
    };

    public FlightService(IFlightRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<FlightDto>> GetAllAsync()
    {
        var flights = await _repository.GetAllAsync();
        return flights.Select(MapToDto);
    }

    public async Task<FlightDto?> GetByIdAsync(int id)
    {
        var flight = await _repository.GetByIdAsync(id);
        return flight != null ? MapToDto(flight) : null;
    }

    public async Task<FlightDto> CreateAsync(CreateFlightDto dto)
    {
        var existing = await _repository.GetByFlightNumberAsync(dto.FlightNumber);
        if (existing != null)
            throw new InvalidOperationException($"Flight number {dto.FlightNumber} already exists");

        if (dto.DepartureTime <= DateTime.UtcNow)
            throw new ArgumentException("Departure time must be in the future", nameof(dto.DepartureTime));

        if (dto.ArrivalTime <= dto.DepartureTime)
            throw new ArgumentException("Arrival time must be after departure time", nameof(dto.ArrivalTime));

        if (!ValidateAirportCode(dto.DepartureAirport))
            throw new ArgumentException("Invalid departure airport code", nameof(dto.DepartureAirport));

        if (!ValidateAirportCode(dto.ArrivalAirport))
            throw new ArgumentException("Invalid arrival airport code", nameof(dto.ArrivalAirport));

        if (!ValidateDifferentAirports(dto.DepartureAirport, dto.ArrivalAirport))
            throw new ArgumentException("Departure and arrival airports must be different");

        var durationMinutes = (int)(dto.ArrivalTime - dto.DepartureTime).TotalMinutes;
        if (!ValidateFlightDuration(durationMinutes))
            throw new ArgumentException("Flight duration must be between 30 minutes and 18 hours");

        var flight = new Flight
        {
            FlightNumber = dto.FlightNumber,
            DepartureAirport = dto.DepartureAirport,
            ArrivalAirport = dto.ArrivalAirport,
            DepartureTime = dto.DepartureTime,
            ArrivalTime = dto.ArrivalTime,
            AircraftType = dto.AircraftType,
            Status = FlightStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(flight);
        return MapToDto(created);
    }

    public async Task<FlightDto> CancelAsync(int id)
    {
        var flight = await _repository.GetByIdAsync(id);
        if (flight == null)
            throw new KeyNotFoundException($"Flight with id {id} not found");

        if (!ValidateStatusTransition(flight.Status, FlightStatus.Cancelled))
            throw new InvalidOperationException($"Cannot cancel flight in {flight.Status} status");

        flight.Status = FlightStatus.Cancelled;
        flight.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(flight);
        return MapToDto(updated);
    }

    public bool ValidateStatusTransition(FlightStatus from, FlightStatus to)
    {
        return ValidTransitions.TryGetValue(from, out var validTargets) && validTargets.Contains(to);
    }

    public bool ValidateFlightDuration(int minutes)
    {
        return minutes >= 30 && minutes <= 1080;
    }

    public bool ValidateAirportCode(string code)
    {
        return !string.IsNullOrEmpty(code) && 
               code.Length == 3 && 
               code.All(char.IsLetter);
    }

    public bool ValidateDifferentAirports(string departure, string arrival)
    {
        return !string.Equals(departure, arrival, StringComparison.OrdinalIgnoreCase);
    }

    private static FlightDto MapToDto(Flight flight)
    {
        return new FlightDto(
            flight.Id,
            flight.FlightNumber,
            flight.DepartureAirport,
            flight.ArrivalAirport,
            flight.DepartureTime,
            flight.ArrivalTime,
            flight.AircraftType,
            flight.Status
        );
    }
}
