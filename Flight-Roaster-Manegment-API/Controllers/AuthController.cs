using FlightRosterAPI.Models;
using FlightRosterAPI.Models.DTOs.User;
using FlightRosterAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightRosterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var response = new ResponseDto();
            try
            {
                var loginResponse = await _userService.LoginAsync(loginDto);

                _logger.LogInformation("Login successful for user: {User}", loginDto.UserNameOrEmail);

                response.IsSuccess = true;
                response.Message = "Giriş başarılı";
                response.Result = loginResponse;
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Login failed: {Message}", ex.Message);
                response.IsSuccess = false;
                response.Message = ex.Message;
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                response.IsSuccess = false;
                response.Message = "Giriş sırasında bir hata oluştu";
                response.Metadata = new { error = ex.Message, inner = ex.InnerException?.Message };
                return StatusCode(500, response);
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            var response = new ResponseDto();
            try
            {
                var user = await _userService.RegisterAsync(registerDto);

                _logger.LogInformation("Registration successful for user: {Email}", registerDto.Email);

                response.IsSuccess = true;
                response.Message = "Kayıt başarıyla oluşturuldu";
                response.Result = user;
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Registration failed: {Message}", ex.Message);
                response.IsSuccess = false;
                response.Message = ex.Message;
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                response.IsSuccess = false;
                response.Message = "Kayıt sırasında bir hata oluştu";
                response.Metadata = new { error = ex.Message, inner = ex.InnerException?.Message };
                return StatusCode(500, response);
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var response = new ResponseDto();
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
                await _userService.ChangePasswordAsync(userId, changePasswordDto);

                _logger.LogInformation("Password changed for user: {UserId}", userId);

                response.IsSuccess = true;
                response.Message = "Şifre başarıyla değiştirildi";
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Password change failed: {Message}", ex.Message);
                response.IsSuccess = false;
                response.Message = ex.Message;
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                response.IsSuccess = false;
                response.Message = "Şifre değiştirme sırasında bir hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var response = new ResponseDto();
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Kullanıcı bulunamadı";
                    return NotFound(response);
                }

                response.IsSuccess = true;
                response.Message = "Kullanıcı bilgileri başarıyla alındı";
                response.Result = user;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                response.IsSuccess = false;
                response.Message = "Kullanıcı bilgileri alınırken hata oluştu";
                return StatusCode(500, response);
            }
        }
    }
}