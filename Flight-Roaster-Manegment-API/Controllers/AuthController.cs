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

        /// <summary>
        /// Kullanıcı girişi yapar ve JWT token döner
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var response = await _userService.LoginAsync(loginDto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Login failed: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "Giriş sırasında bir hata oluştu" });
            }
        }

        /// <summary>
        /// Yeni kullanıcı kaydı oluşturur
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            try
            {
                var user = await _userService.RegisterAsync(registerDto);
                return CreatedAtAction(nameof(GetUserById), "Users", new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Registration failed: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { message = "Kayıt sırasında bir hata oluştu" });
            }
        }

        /// <summary>
        /// Kullanıcı şifresini değiştirir
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
                var result = await _userService.ChangePasswordAsync(userId, changePasswordDto);

                return Ok(new { message = "Şifre başarıyla değiştirildi" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Password change failed: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return StatusCode(500, new { message = "Şifre değiştirme sırasında bir hata oluştu" });
            }
        }

        /// <summary>
        /// Giriş yapmış kullanıcının bilgilerini döner
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                    return NotFound(new { message = "Kullanıcı bulunamadı" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return StatusCode(500, new { message = "Kullanıcı bilgileri alınırken hata oluştu" });
            }
        }

        // Placeholder for Users controller route
        private IActionResult GetUserById(int id) => Ok();
    }
}