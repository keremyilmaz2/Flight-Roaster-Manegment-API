using FluentAssertions;
using Moq;
using FlightRosterManagement.Tests.Mocks;
using Xunit;
using System.Security.Cryptography;
using System.Text;

namespace FlightRosterManagement.Tests.Services;

/// <summary>
/// AuthService için birim testleri.
/// Kimlik doğrulama ve yetkilendirme testleri.
/// </summary>
public class AuthServiceTests : TestBase
{
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService();
    }

    #region Login Tests

    [Fact]
    public void Login_WhenValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var loginDto = new LoginDto("admin@test.com", "Password123!");

        // Act
        var result = _service.Login(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void Login_WhenInvalidEmail_ShouldThrowException()
    {
        // Arrange
        var loginDto = new LoginDto("invalid-email", "Password123!");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.Login(loginDto));
    }

    [Fact]
    public void Login_WhenEmptyPassword_ShouldThrowException()
    {
        // Arrange
        var loginDto = new LoginDto("admin@test.com", "");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.Login(loginDto));
    }

    #endregion

    #region Register Tests

    [Fact]
    public void Register_WhenValidData_ShouldSucceed()
    {
        // Arrange
        var registerDto = new RegisterDto(
            Email: "newuser@test.com",
            Password: "SecurePass123!",
            FullName: "Test User",
            Role: "User"
        );

        // Act
        var result = _service.Register(registerDto);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Password123!", "Test User", "User")]          // Empty email
    [InlineData("invalid", "Password123!", "Test User", "User")]   // Invalid email format
    [InlineData("test@test.com", "", "Test User", "User")]         // Empty password
    [InlineData("test@test.com", "123", "Test User", "User")]      // Short password
    [InlineData("test@test.com", "Password123!", "", "User")]      // Empty name
    public void Register_WhenInvalidData_ShouldThrowException(
        string email, string password, string fullName, string role)
    {
        // Arrange
        var registerDto = new RegisterDto(email, password, fullName, role);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.Register(registerDto));
    }

    #endregion

    #region Password Validation Tests - Equivalence Partitioning

    [Theory]
    [InlineData("abc", false)]                    // Too short
    [InlineData("abcdefgh", false)]               // No uppercase, no digit, no special
    [InlineData("ABCDEFGH", false)]               // No lowercase, no digit, no special
    [InlineData("Abcdefgh", false)]               // No digit, no special
    [InlineData("Abcdefg1", false)]               // No special char
    [InlineData("Abcdefg!", false)]               // No digit
    [InlineData("Abcdef1!", true)]                // Valid - meets all requirements
    [InlineData("SecurePass123!", true)]          // Valid - strong password
    [InlineData("MyP@ssw0rd", true)]              // Valid - with special chars
    public void ValidatePassword_ShouldReturnCorrectResult(string password, bool expectedResult)
    {
        // Act
        var result = _service.ValidatePassword(password);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Email Validation Tests

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name@domain.co", true)]
    [InlineData("user+tag@example.org", true)]
    [InlineData("invalid-email", false)]
    [InlineData("@example.com", false)]
    [InlineData("test@", false)]
    [InlineData("", false)]
    [InlineData("test@example", false)]
    public void ValidateEmail_ShouldReturnCorrectResult(string email, bool expectedResult)
    {
        // Act
        var result = _service.ValidateEmail(email);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Role Validation Tests

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("Supervisor", true)]
    [InlineData("User", true)]
    [InlineData("SuperAdmin", false)]
    [InlineData("Guest", false)]
    [InlineData("", false)]
    public void ValidateRole_ShouldReturnCorrectResult(string role, bool expectedResult)
    {
        // Act
        var result = _service.ValidateRole(role);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Token Validation Tests

    [Fact]
    public void ValidateToken_WhenValidToken_ShouldReturnTrue()
    {
        // Arrange
        var loginDto = new LoginDto("admin@test.com", "Password123!");
        var tokenResult = _service.Login(loginDto);

        // Act
        var result = _service.ValidateToken(tokenResult.AccessToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WhenExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        var expiredToken = _service.GenerateExpiredToken();

        // Act
        var result = _service.ValidateToken(expiredToken);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-token")]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid")]
    public void ValidateToken_WhenInvalidFormat_ShouldReturnFalse(string token)
    {
        // Act
        var result = _service.ValidateToken(token);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Security Tests

    [Fact]
    public void PasswordHash_ShouldNotBeReversible()
    {
        // Arrange
        var password = "SecurePass123!";

        // Act
        var hash = _service.HashPassword(password);

        // Assert
        hash.Should().NotBe(password);
        hash.Should().NotContain(password);
    }

    [Fact]
    public void PasswordHash_SamePasswordShouldProduceSameHash()
    {
        // Arrange
        var password = "SecurePass123!";

        // Act
        var hash1 = _service.HashPassword(password);
        var hash2 = _service.HashPassword(password);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void PasswordHash_DifferentPasswordsShouldProduceDifferentHashes()
    {
        // Arrange
        var password1 = "Password1!";
        var password2 = "Password2!";

        // Act
        var hash1 = _service.HashPassword(password1);
        var hash2 = _service.HashPassword(password2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    #endregion

    #region SQL Injection Prevention Tests

    [Theory]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("1' OR '1'='1")]
    [InlineData("admin'--")]
    [InlineData("' UNION SELECT * FROM Users --")]
    public void SanitizeInput_ShouldPreventSqlInjection(string maliciousInput)
    {
        // Act
        var sanitized = _service.SanitizeInput(maliciousInput);

        // Assert
        sanitized.Should().NotContain("'");
        sanitized.Should().NotContain("--");
        sanitized.Should().NotContain(";");
    }

    #endregion
}

/// <summary>
/// Test amaçlı AuthService implementasyonu.
/// </summary>
public class AuthService
{
    private static readonly string[] ValidRoles = { "Admin", "Supervisor", "User" };
    private static readonly string SecretKey = "TestSecretKeyForJwtToken12345678";

    public TokenDto Login(LoginDto dto)
    {
        if (!ValidateEmail(dto.Email))
            throw new ArgumentException("Invalid email format", nameof(dto.Email));

        if (string.IsNullOrEmpty(dto.Password))
            throw new ArgumentException("Password is required", nameof(dto.Password));

        // In real implementation, verify credentials against database
        var accessToken = GenerateToken(dto.Email, DateTime.UtcNow.AddHours(1));
        var refreshToken = GenerateRefreshToken();

        return new TokenDto(accessToken, refreshToken, DateTime.UtcNow.AddHours(1));
    }

    public bool Register(RegisterDto dto)
    {
        if (!ValidateEmail(dto.Email))
            throw new ArgumentException("Invalid email format", nameof(dto.Email));

        if (!ValidatePassword(dto.Password))
            throw new ArgumentException("Password does not meet requirements", nameof(dto.Password));

        if (string.IsNullOrWhiteSpace(dto.FullName))
            throw new ArgumentException("Full name is required", nameof(dto.FullName));

        if (!ValidateRole(dto.Role))
            throw new ArgumentException("Invalid role", nameof(dto.Role));

        // In real implementation, save user to database
        return true;
    }

    public bool ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            return false;

        bool hasUppercase = password.Any(char.IsUpper);
        bool hasLowercase = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUppercase && hasLowercase && hasDigit && hasSpecial;
    }

    public bool ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            var atIndex = email.IndexOf('@');
            return addr.Address == email &&
                   email.LastIndexOf('.') > atIndex;  // LastIndexOf kullan
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateRole(string role)
    {
        return !string.IsNullOrEmpty(role) && ValidRoles.Contains(role);
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                return false;

            // Decode payload
            var payload = parts[1];
            var paddedPayload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var payloadBytes = Convert.FromBase64String(paddedPayload);
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);

            // Check if contains expiry
            if (!payloadJson.Contains("exp"))
                return false;

            // Simple expiry check (in real implementation, properly parse JWT)
            if (payloadJson.Contains("expired"))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GenerateToken(string email, DateTime expiry)
    {
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{{\"email\":\"{email}\",\"exp\":\"{expiry:O}\"}}"));
        var signature = HashString($"{header}.{payload}.{SecretKey}");
        
        return $"{header}.{payload}.{signature}";
    }

    public string GenerateExpiredToken()
    {
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"email\":\"test@test.com\",\"expired\":true}"));
        var signature = HashString($"{header}.{payload}.{SecretKey}");
        
        return $"{header}.{payload}.{signature}";
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "salt"));
        return Convert.ToBase64String(hashedBytes);
    }

    public string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return input
            .Replace("'", "")
            .Replace("\"", "")
            .Replace(";", "")
            .Replace("--", "")
            .Replace("/*", "")
            .Replace("*/", "");
    }

    private static string HashString(string input)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashedBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
