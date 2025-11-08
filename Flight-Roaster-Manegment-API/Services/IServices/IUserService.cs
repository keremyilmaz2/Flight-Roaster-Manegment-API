using FlightRosterAPI.Models.DTOs.User;
using FlightRosterAPI.Models.Entities;

namespace FlightRosterAPI.Services.IServices
{
    public interface IUserService
    {
        // Authentication
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
        Task<UserResponseDto> RegisterAsync(RegisterUserDto registerDto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);

        // User Management
        Task<UserResponseDto?> GetUserByIdAsync(int userId);
        Task<UserResponseDto?> GetUserByUsernameAsync(string username);
        Task<UserResponseDto?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto> UpdateUserAsync(int userId, UpdateUserDto updateDto);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ActivateUserAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);

        // Role Management
        Task<IEnumerable<UserResponseDto>> GetUsersByRoleAsync(string roleName);
    }
}