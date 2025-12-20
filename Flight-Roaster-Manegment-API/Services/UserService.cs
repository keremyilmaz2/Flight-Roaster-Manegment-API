using Azure;
using FlightRosterAPI.Models;
using FlightRosterAPI.Models.DTOs.Passenger;
using FlightRosterAPI.Models.DTOs.User;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Models.Enums;
using FlightRosterAPI.Services.IServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FlightRosterAPI.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IPassengerService _passengerService;
        private readonly JwtSettings _jwtSettings;

        public UserService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole<int>> roleManager,
            IPassengerService passengerService,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _passengerService = passengerService;
        }

        #region Authentication

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Find user by email or username
            var user = await _userManager.FindByEmailAsync(loginDto.UserNameOrEmail)
                       ?? await _userManager.FindByNameAsync(loginDto.UserNameOrEmail);

            if (user == null)
                throw new UnauthorizedAccessException("Kullanıcı adı veya şifre hatalı");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Kullanıcı hesabı pasif durumda");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
                throw new UnauthorizedAccessException("Kullanıcı adı veya şifre hatalı");

            var token = await GenerateJwtTokenAsync(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

            return new LoginResponseDto
            {
                Token = token,
                User = MapToUserResponseDto(user),
                ExpiresAt = expiresAt
            };
        }

        public async Task<UserResponseDto> RegisterAsync(RegisterUserDto registerDto)
        {
            // Check if user exists
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Bu email adresi zaten kullanılıyor");

            existingUser = await _userManager.FindByNameAsync(registerDto.UserName);
            if (existingUser != null)
                throw new InvalidOperationException("Bu kullanıcı adı zaten kullanılıyor");

            // Create user
            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                Nationality = registerDto.Nationality,
                Gender = registerDto.Gender,
                DateOfBirth = registerDto.DateOfBirth,
                Role = registerDto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Kullanıcı oluşturulurken hata oluştu: {errors}");
            }

            // Ensure role exists
            var roleName = registerDto.Role.ToString();
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
            }

            // Add user to role
            await _userManager.AddToRoleAsync(user, roleName);

            if (user.Role == UserRole.Passenger)
            {
                var createPassengerDto = new CreatePassengerDto
                {
                    UserId = user.Id,
                    // DTO'dan gelen değerleri kullan (eğer varsa)
                    PassportNumber = registerDto.PassportNumber ?? string.Empty,
                    NationalIdNumber = registerDto.NationalIdNumber ?? string.Empty
                };

                await _passengerService.CreatePassengerAsync(createPassengerDto);
            }
            return MapToUserResponseDto(user);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new KeyNotFoundException("Kullanıcı bulunamadı");

            var result = await _userManager.ChangePasswordAsync(
                user,
                changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Şifre değiştirilemedi: {errors}");
            }

            return true;
        }

        #endregion

        #region User Management

        public async Task<UserResponseDto?> GetUserByIdAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user != null ? MapToUserResponseDto(user) : null;
        }

        public async Task<UserResponseDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return user != null ? MapToUserResponseDto(user) : null;
        }

        public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null ? MapToUserResponseDto(user) : null;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();
            return users.Select(MapToUserResponseDto);
        }

        public async Task<UserResponseDto> UpdateUserAsync(int userId, UpdateUserDto updateDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new KeyNotFoundException("Kullanıcı bulunamadı");

            if (!string.IsNullOrEmpty(updateDto.FirstName))
                user.FirstName = updateDto.FirstName;

            if (!string.IsNullOrEmpty(updateDto.LastName))
                user.LastName = updateDto.LastName;

            if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != user.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(updateDto.Email);
                if (existingUser != null && existingUser.Id != userId)
                    throw new InvalidOperationException("Bu email adresi başka bir kullanıcı tarafından kullanılıyor");
                user.Email = updateDto.Email;
            }

            if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
                user.PhoneNumber = updateDto.PhoneNumber;

            if (!string.IsNullOrEmpty(updateDto.Nationality))
                user.Nationality = updateDto.Nationality;
            if (!string.IsNullOrEmpty(updateDto.Gender))
                user.Gender = updateDto.Gender;

            if (updateDto.DateOfBirth.HasValue)
                user.DateOfBirth = updateDto.DateOfBirth.Value;

            if (updateDto.IsActive.HasValue)
                user.IsActive = updateDto.IsActive.Value;

            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Kullanıcı güncellenemedi: {errors}");
            }

            return MapToUserResponseDto(user);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new KeyNotFoundException("Kullanıcı bulunamadı");

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ActivateUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new KeyNotFoundException("Kullanıcı bulunamadı");

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new KeyNotFoundException("Kullanıcı bulunamadı");

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<IEnumerable<UserResponseDto>> GetUsersByRoleAsync(string roleName)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            return users.Select(MapToUserResponseDto);
        }

        #endregion

        #region Private Methods

        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim("Role", user.Role.ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserResponseDto MapToUserResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber,
                Nationality = user.Nationality,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }

        #endregion
    }
}