using FlightRosterAPI.Models.DTOs.Pilot;
using FlightRosterAPI.Models.Enums;
using FlightRosterAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightRosterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PilotsController : ControllerBase
    {
        private readonly IPilotService _pilotService;
        private readonly ILogger<PilotsController> _logger;

        public PilotsController(
            IPilotService pilotService,
            ILogger<PilotsController> logger)
        {
            _pilotService = pilotService;
            _logger = logger;
        }

        /// <summary>
        /// Tüm pilotları getirir
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PilotResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPilots()
        {
            try
            {
                var pilots = await _pilotService.GetAllPilotsAsync();
                return Ok(pilots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all pilots");
                return StatusCode(500, new { message = "Pilotlar getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Aktif pilotları getirir
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<PilotResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActivePilots()
        {
            try
            {
                var pilots = await _pilotService.GetActivePilotsAsync();
                return Ok(pilots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active pilots");
                return StatusCode(500, new { message = "Aktif pilotlar getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// ID'ye göre pilot getirir
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PilotResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPilotById(int id)
        {
            try
            {
                var pilot = await _pilotService.GetPilotByIdAsync(id);
                if (pilot == null)
                    return NotFound(new { message = "Pilot bulunamadı" });

                return Ok(pilot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pilot {PilotId}", id);
                return StatusCode(500, new { message = "Pilot getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Kullanıcı ID'sine göre pilot getirir
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(PilotResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPilotByUserId(int userId)
        {
            try
            {
                var pilot = await _pilotService.GetPilotByUserIdAsync(userId);
                if (pilot == null)
                    return NotFound(new { message = "Pilot bulunamadı" });

                return Ok(pilot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pilot by user {UserId}", userId);
                return StatusCode(500, new { message = "Pilot getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Lisans numarasına göre pilot getirir
        /// </summary>
        [HttpGet("license/{licenseNumber}")]
        [ProducesResponseType(typeof(PilotResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPilotByLicenseNumber(string licenseNumber)
        {
            try
            {
                var pilot = await _pilotService.GetPilotByLicenseNumberAsync(licenseNumber);
                if (pilot == null)
                    return NotFound(new { message = "Pilot bulunamadı" });

                return Ok(pilot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pilot by license {LicenseNumber}", licenseNumber);
                return StatusCode(500, new { message = "Pilot getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Kıdeme göre pilotları getirir
        /// </summary>
        [HttpGet("seniority/{seniority}")]
        [ProducesResponseType(typeof(IEnumerable<PilotResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPilotsBySeniority(PilotSeniority seniority)
        {
            try
            {
                var pilots = await _pilotService.GetPilotsBySeniorityAsync(seniority);
                return Ok(pilots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pilots by seniority {Seniority}", seniority);
                return StatusCode(500, new { message = "Pilotlar getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Uçuş için uygun pilotları getirir
        /// </summary>
        [HttpGet("available-for-flight/{flightId}")]
        [ProducesResponseType(typeof(IEnumerable<PilotResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAvailablePilotsForFlight(int flightId)
        {
            try
            {
                var pilots = await _pilotService.GetAvailablePilotsForFlightAsync(flightId);
                return Ok(pilots);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available pilots for flight {FlightId}", flightId);
                return StatusCode(500, new { message = "Uygun pilotlar getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Lisansı süresi dolmuş pilotları getirir
        /// </summary>
        [HttpGet("expired-licenses")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(IEnumerable<PilotResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPilotsWithExpiredLicenses()
        {
            try
            {
                var pilots = await _pilotService.GetPilotsWithExpiredLicensesAsync();
                return Ok(pilots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pilots with expired licenses");
                return StatusCode(500, new { message = "Lisansı dolmuş pilotlar getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Yeni pilot oluşturur
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(PilotResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePilot([FromBody] CreatePilotDto createDto)
        {
            try
            {
                var pilot = await _pilotService.CreatePilotAsync(createDto);
                return CreatedAtAction(nameof(GetPilotById), new { id = pilot.PilotId }, pilot);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pilot");
                return StatusCode(500, new { message = "Pilot oluşturulurken hata oluştu" });
            }
        }

        /// <summary>
        /// Pilot bilgilerini günceller
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(PilotResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePilot(int id, [FromBody] UpdatePilotDto updateDto)
        {
            try
            {
                var pilot = await _pilotService.UpdatePilotAsync(id, updateDto);
                return Ok(pilot);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pilot {PilotId}", id);
                return StatusCode(500, new { message = "Pilot güncellenirken hata oluştu" });
            }
        }

        /// <summary>
        /// Pilotu siler
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePilot(int id)
        {
            try
            {
                await _pilotService.DeletePilotAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting pilot {PilotId}", id);
                return StatusCode(500, new { message = "Pilot silinirken hata oluştu" });
            }
        }
    }
}