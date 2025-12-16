using FluentAssertions;
using Moq;
using FlightRosterManagement.Tests.Mocks;
using Xunit;

namespace FlightRosterManagement.Tests.Services;

/// <summary>
/// PilotService için birim testleri.
/// Beyaz kutu test yaklaşımı ile tüm metodlar test edilmektedir.
/// </summary>
public class PilotServiceTests : TestBase
{
    private readonly Mock<IPilotRepository> _mockRepository;
    private readonly PilotService _service;

    public PilotServiceTests()
    {
        _mockRepository = new Mock<IPilotRepository>();
        _service = new PilotService(_mockRepository.Object);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenPilotsExist_ShouldReturnAllPilots()
    {
        // Arrange
        var pilots = new List<Pilot>
        {
            CreateTestPilot(1, "Ahmet", "Yılmaz"),
            CreateTestPilot(2, "Mehmet", "Kaya"),
            CreateTestPilot(3, "Ali", "Demir")
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(pilots);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoPilots_ShouldReturnEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Pilot>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenPilotExists_ShouldReturnPilot()
    {
        // Arrange
        var pilot = CreateTestPilot(1, "Ahmet", "Yılmaz");
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pilot);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Ahmet");
        result.LastName.Should().Be("Yılmaz");
    }

    [Fact]
    public async Task GetByIdAsync_WhenPilotNotExists_ShouldReturnNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Pilot?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetByIdAsync_WhenIdIsInvalid_ShouldThrowArgumentException(int invalidId)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(invalidId));
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WhenValidData_ShouldCreatePilot()
    {
        // Arrange
        var createDto = new CreatePilotDto(
            FirstName: "Ahmet",
            LastName: "Yılmaz",
            EmployeeNumber: "PLT001",
            DateOfBirth: new DateTime(1985, 5, 15),
            LicenseNumber: "LIC001",
            LicenseExpiryDate: DateTime.UtcNow.AddYears(2),
            TotalFlightHours: 5000,
            Rank: "Captain"
        );

        _mockRepository.Setup(r => r.GetByEmployeeNumberAsync("PLT001")).ReturnsAsync((Pilot?)null);
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Pilot>()))
            .ReturnsAsync((Pilot p) => { p.Id = 1; return p; });

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.FirstName.Should().Be("Ahmet");
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Pilot>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenEmployeeNumberExists_ShouldThrowException()
    {
        // Arrange
        var existingPilot = CreateTestPilot(1, "Existing", "Pilot");
        var createDto = new CreatePilotDto(
            FirstName: "Ahmet",
            LastName: "Yılmaz",
            EmployeeNumber: "PLT001",
            DateOfBirth: new DateTime(1985, 5, 15),
            LicenseNumber: "LIC001",
            LicenseExpiryDate: DateTime.UtcNow.AddYears(2),
            TotalFlightHours: 5000,
            Rank: "Captain"
        );

        _mockRepository.Setup(r => r.GetByEmployeeNumberAsync("PLT001")).ReturnsAsync(existingPilot);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WhenLicenseExpired_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreatePilotDto(
            FirstName: "Ahmet",
            LastName: "Yılmaz",
            EmployeeNumber: "PLT001",
            DateOfBirth: new DateTime(1985, 5, 15),
            LicenseNumber: "LIC001",
            LicenseExpiryDate: DateTime.UtcNow.AddDays(-1), // Expired license
            TotalFlightHours: 5000,
            Rank: "Captain"
        );

        _mockRepository.Setup(r => r.GetByEmployeeNumberAsync("PLT001")).ReturnsAsync((Pilot?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(createDto));
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WhenValidData_ShouldUpdatePilot()
    {
        // Arrange
        var existingPilot = CreateTestPilot(1, "Ahmet", "Yılmaz");
        var updateDto = new UpdatePilotDto(
            FirstName: "Ahmet Updated",
            LastName: "Yılmaz Updated",
            LicenseNumber: "LIC001-NEW",
            LicenseExpiryDate: DateTime.UtcNow.AddYears(3),
            TotalFlightHours: 6000,
            Rank: "Captain",
            IsActive: true
        );

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingPilot);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Pilot>()))
            .ReturnsAsync((Pilot p) => p);

        // Act
        var result = await _service.UpdateAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Ahmet Updated");
        result.TotalFlightHours.Should().Be(6000);
    }

    [Fact]
    public async Task UpdateAsync_WhenPilotNotExists_ShouldThrowException()
    {
        // Arrange
        var updateDto = new UpdatePilotDto(
            FirstName: "Test",
            LastName: "Test",
            LicenseNumber: "LIC001",
            LicenseExpiryDate: DateTime.UtcNow.AddYears(1),
            TotalFlightHours: 1000,
            Rank: "FirstOfficer",
            IsActive: true
        );

        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Pilot?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(999, updateDto));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenPilotExists_ShouldReturnTrue()
    {
        // Arrange
        _mockRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenPilotNotExists_ShouldThrowException()
    {
        // Arrange
        _mockRepository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(999));
    }

    #endregion

    #region Validation Tests - Equivalence Partitioning

    [Theory]
    [InlineData(20, false)]  // Geçersiz - minimum yaş altı
    [InlineData(21, true)]   // Geçerli - minimum yaş
    [InlineData(35, true)]   // Geçerli - normal yaş
    [InlineData(65, true)]   // Geçerli - maksimum yaş
    [InlineData(66, false)]  // Geçersiz - maksimum yaş üstü
    public void ValidatePilotAge_ShouldReturnCorrectResult(int age, bool expectedResult)
    {
        // Arrange
        var birthDate = DateTime.UtcNow.AddYears(-age);

        // Act
        var result = _service.ValidatePilotAge(birthDate);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(-1, false)]     // Geçersiz - negatif
    [InlineData(0, true)]       // Geçerli - minimum
    [InlineData(5000, true)]    // Geçerli - normal
    [InlineData(50000, true)]   // Geçerli - maksimum
    [InlineData(50001, false)]  // Geçersiz - maksimum üstü
    public void ValidateFlightHours_ShouldReturnCorrectResult(int hours, bool expectedResult)
    {
        // Act
        var result = _service.ValidateFlightHours(hours);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Helper Methods

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

    #endregion
}

/// <summary>
/// Test amaçlı PilotService implementasyonu.
/// Gerçek projede bu sınıf ana projede olacaktır.
/// </summary>
public class PilotService
{
    private readonly IPilotRepository _repository;

    public PilotService(IPilotRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PilotDto>> GetAllAsync()
    {
        var pilots = await _repository.GetAllAsync();
        return pilots.Select(MapToDto);
    }

    public async Task<PilotDto?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be greater than zero", nameof(id));

        var pilot = await _repository.GetByIdAsync(id);
        return pilot != null ? MapToDto(pilot) : null;
    }

    public async Task<PilotDto> CreateAsync(CreatePilotDto dto)
    {
        // Validate employee number uniqueness
        var existing = await _repository.GetByEmployeeNumberAsync(dto.EmployeeNumber);
        if (existing != null)
            throw new InvalidOperationException($"Employee number {dto.EmployeeNumber} already exists");

        // Validate license expiry
        if (dto.LicenseExpiryDate <= DateTime.UtcNow)
            throw new ArgumentException("License has expired", nameof(dto.LicenseExpiryDate));

        // Validate age
        if (!ValidatePilotAge(dto.DateOfBirth))
            throw new ArgumentException("Pilot age is not within valid range (21-65)", nameof(dto.DateOfBirth));

        // Validate flight hours
        if (!ValidateFlightHours(dto.TotalFlightHours))
            throw new ArgumentException("Flight hours must be between 0 and 50000", nameof(dto.TotalFlightHours));

        var pilot = new Pilot
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EmployeeNumber = dto.EmployeeNumber,
            DateOfBirth = dto.DateOfBirth,
            LicenseNumber = dto.LicenseNumber,
            LicenseExpiryDate = dto.LicenseExpiryDate,
            TotalFlightHours = dto.TotalFlightHours,
            Rank = dto.Rank,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(pilot);
        return MapToDto(created);
    }

    public async Task<PilotDto> UpdateAsync(int id, UpdatePilotDto dto)
    {
        var pilot = await _repository.GetByIdAsync(id);
        if (pilot == null)
            throw new KeyNotFoundException($"Pilot with id {id} not found");

        pilot.FirstName = dto.FirstName;
        pilot.LastName = dto.LastName;
        pilot.LicenseNumber = dto.LicenseNumber;
        pilot.LicenseExpiryDate = dto.LicenseExpiryDate;
        pilot.TotalFlightHours = dto.TotalFlightHours;
        pilot.Rank = dto.Rank;
        pilot.IsActive = dto.IsActive;
        pilot.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(pilot);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var exists = await _repository.ExistsAsync(id);
        if (!exists)
            throw new KeyNotFoundException($"Pilot with id {id} not found");

        return await _repository.DeleteAsync(id);
    }

    public bool ValidatePilotAge(DateTime dateOfBirth)
    {
        var age = DateTime.UtcNow.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.UtcNow.AddYears(-age)) age--;
        return age >= 21 && age <= 65;
    }

    public bool ValidateFlightHours(int hours)
    {
        return hours >= 0 && hours <= 50000;
    }

    private static PilotDto MapToDto(Pilot pilot)
    {
        return new PilotDto(
            pilot.Id,
            pilot.FirstName,
            pilot.LastName,
            pilot.EmployeeNumber,
            pilot.DateOfBirth,
            pilot.LicenseNumber,
            pilot.LicenseExpiryDate,
            pilot.TotalFlightHours,
            pilot.Rank,
            pilot.IsActive
        );
    }
}
