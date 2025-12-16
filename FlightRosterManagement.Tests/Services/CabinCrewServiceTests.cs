using FluentAssertions;
using Moq;
using FlightRosterManagement.Tests.Mocks;
using Xunit;

namespace FlightRosterManagement.Tests.Services;

/// <summary>
/// CabinCrewService için birim testleri.
/// Kabin ekibi CRUD işlemleri ve iş kuralları test edilmektedir.
/// </summary>
public class CabinCrewServiceTests : TestBase
{
    private readonly Mock<ICabinCrewRepository> _mockRepository;
    private readonly CabinCrewService _service;

    public CabinCrewServiceTests()
    {
        _mockRepository = new Mock<ICabinCrewRepository>();
        _service = new CabinCrewService(_mockRepository.Object);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenCrewExists_ShouldReturnAllCrew()
    {
        // Arrange
        var crew = new List<CabinCrew>
        {
            CreateTestCabinCrew(1, "Ayşe", "Demir"),
            CreateTestCabinCrew(2, "Fatma", "Kaya"),
            CreateTestCabinCrew(3, "Zeynep", "Yıldız")
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(crew);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoCrew_ShouldReturnEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<CabinCrew>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenCrewExists_ShouldReturnCrew()
    {
        // Arrange
        var crew = CreateTestCabinCrew(1, "Ayşe", "Demir");
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(crew);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Ayşe");
    }

    [Fact]
    public async Task GetByIdAsync_WhenCrewNotExists_ShouldReturnNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((CabinCrew?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WhenValidData_ShouldCreateCrew()
    {
        // Arrange
        var createDto = new CreateCabinCrewDto(
            FirstName: "Ayşe",
            LastName: "Demir",
            EmployeeNumber: "CC001",
            DateOfBirth: new DateTime(1990, 3, 20),
            Position: "Purser",
            Languages: new List<string> { "Turkish", "English", "German" },
            CertificationExpiryDate: DateTime.UtcNow.AddYears(1)
        );

        _mockRepository.Setup(r => r.GetByEmployeeNumberAsync("CC001")).ReturnsAsync((CabinCrew?)null);
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<CabinCrew>()))
            .ReturnsAsync((CabinCrew c) => { c.Id = 1; return c; });

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Languages.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateAsync_WhenEmployeeNumberExists_ShouldThrowException()
    {
        // Arrange
        var existingCrew = CreateTestCabinCrew(1, "Existing", "Crew");
        var createDto = new CreateCabinCrewDto(
            FirstName: "Ayşe",
            LastName: "Demir",
            EmployeeNumber: "CC001",
            DateOfBirth: new DateTime(1990, 3, 20),
            Position: "Purser",
            Languages: new List<string> { "Turkish" },
            CertificationExpiryDate: DateTime.UtcNow.AddYears(1)
        );

        _mockRepository.Setup(r => r.GetByEmployeeNumberAsync("CC001")).ReturnsAsync(existingCrew);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WhenNoLanguages_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreateCabinCrewDto(
            FirstName: "Ayşe",
            LastName: "Demir",
            EmployeeNumber: "CC001",
            DateOfBirth: new DateTime(1990, 3, 20),
            Position: "Purser",
            Languages: new List<string>(), // Empty languages
            CertificationExpiryDate: DateTime.UtcNow.AddYears(1)
        );

        _mockRepository.Setup(r => r.GetByEmployeeNumberAsync("CC001")).ReturnsAsync((CabinCrew?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(createDto));
    }

    #endregion

    #region Position Validation Tests

    [Theory]
    [InlineData("Purser", true)]
    [InlineData("SeniorCrew", true)]
    [InlineData("JuniorCrew", true)]
    [InlineData("InvalidPosition", false)]
    [InlineData("", false)]
    public void ValidatePosition_ShouldReturnCorrectResult(string position, bool expectedResult)
    {
        // Act
        var result = _service.ValidatePosition(position);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Language Validation Tests

    [Fact]
    public void ValidateLanguages_WhenHasRequiredLanguages_ShouldReturnTrue()
    {
        // Arrange
        var languages = new List<string> { "Turkish", "English" };

        // Act
        var result = _service.ValidateLanguages(languages);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateLanguages_WhenMissingTurkish_ShouldReturnFalse()
    {
        // Arrange
        var languages = new List<string> { "English", "German" };

        // Act
        var result = _service.ValidateLanguages(languages);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateLanguages_WhenEmpty_ShouldReturnFalse()
    {
        // Arrange
        var languages = new List<string>();

        // Act
        var result = _service.ValidateLanguages(languages);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Age Validation Tests - Boundary Value Analysis

    [Theory]
    [InlineData(17, false)]  // Minimum altı
    [InlineData(18, true)]   // Minimum değer
    [InlineData(19, true)]   // Minimum üstü
    [InlineData(30, true)]   // Normal değer
    [InlineData(59, true)]   // Maksimum altı
    [InlineData(60, true)]   // Maksimum değer
    [InlineData(61, false)]  // Maksimum üstü
    public void ValidateCabinCrewAge_BoundaryValues_ShouldReturnCorrectResult(int age, bool expectedResult)
    {
        // Arrange
        var birthDate = DateTime.UtcNow.AddYears(-age);

        // Act
        var result = _service.ValidateCabinCrewAge(birthDate);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Helper Methods

    private static CabinCrew CreateTestCabinCrew(int id, string firstName, string lastName)
    {
        return new CabinCrew
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            EmployeeNumber = $"CC{id:D3}",
            DateOfBirth = new DateTime(1990, 1, 1),
            Position = "SeniorCrew",
            Languages = new List<string> { "Turkish", "English" },
            CertificationExpiryDate = DateTime.UtcNow.AddYears(1),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}

/// <summary>
/// Test amaçlı CabinCrewService implementasyonu.
/// </summary>
public class CabinCrewService
{
    private readonly ICabinCrewRepository _repository;
    private static readonly string[] ValidPositions = { "Purser", "SeniorCrew", "JuniorCrew" };

    public CabinCrewService(ICabinCrewRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CabinCrewDto>> GetAllAsync()
    {
        var crew = await _repository.GetAllAsync();
        return crew.Select(MapToDto);
    }

    public async Task<CabinCrewDto?> GetByIdAsync(int id)
    {
        var crew = await _repository.GetByIdAsync(id);
        return crew != null ? MapToDto(crew) : null;
    }

    public async Task<CabinCrewDto> CreateAsync(CreateCabinCrewDto dto)
    {
        var existing = await _repository.GetByEmployeeNumberAsync(dto.EmployeeNumber);
        if (existing != null)
            throw new InvalidOperationException($"Employee number {dto.EmployeeNumber} already exists");

        if (!ValidateLanguages(dto.Languages))
            throw new ArgumentException("At least Turkish language is required", nameof(dto.Languages));

        if (!ValidatePosition(dto.Position))
            throw new ArgumentException("Invalid position", nameof(dto.Position));

        if (!ValidateCabinCrewAge(dto.DateOfBirth))
            throw new ArgumentException("Cabin crew age must be between 18 and 60", nameof(dto.DateOfBirth));

        var crew = new CabinCrew
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EmployeeNumber = dto.EmployeeNumber,
            DateOfBirth = dto.DateOfBirth,
            Position = dto.Position,
            Languages = dto.Languages,
            CertificationExpiryDate = dto.CertificationExpiryDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(crew);
        return MapToDto(created);
    }

    public bool ValidatePosition(string position)
    {
        return !string.IsNullOrEmpty(position) && ValidPositions.Contains(position);
    }

    public bool ValidateLanguages(List<string> languages)
    {
        return languages.Any() && languages.Contains("Turkish");
    }

    public bool ValidateCabinCrewAge(DateTime dateOfBirth)
    {
        var age = DateTime.UtcNow.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.UtcNow.AddYears(-age)) age--;
        return age >= 18 && age <= 60;
    }

    private static CabinCrewDto MapToDto(CabinCrew crew)
    {
        return new CabinCrewDto(
            crew.Id,
            crew.FirstName,
            crew.LastName,
            crew.EmployeeNumber,
            crew.DateOfBirth,
            crew.Position,
            crew.Languages,
            crew.CertificationExpiryDate,
            crew.IsActive
        );
    }
}
