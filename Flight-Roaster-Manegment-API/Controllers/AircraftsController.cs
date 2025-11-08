using FlightRosterAPI.Models.DTOs.Aircraft;
using FlightRosterAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightRosterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AircraftsController : ControllerBase
    {
        private readonly IAircraftService _aircraftService;
        private readonly ILogger<AircraftsController> _logger;

        public AircraftsController(
            IAircraftService aircraftService,
            ILogger<AircraftsController> logger)
        {
            _aircraftService = aircraftService;
            _logger = logger;
        }

        /// <summary>
        /// Tüm uçakları getirir
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AircraftResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAircrafts()
        {
            try
            {
                var aircrafts = await _aircraftService.GetAllAircraftsAsync();
                return Ok(aircrafts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all aircrafts");
                return StatusCode(500, new { message = "Uçaklar getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Aktif uçakları getirir
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<AircraftResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveAircrafts()
        {
            try
            {
                var aircrafts = await _aircraftService.GetActiveAircraftsAsync();
                return Ok(aircrafts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active aircrafts");
                return StatusCode(500, new { message = "Aktif uçaklar getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// ID'ye göre uçak getirir
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AircraftResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAircraftById(int id)
        {
            try
            {
                var aircraft = await _aircraftService.GetAircraftByIdAsync(id);
                if (aircraft == null)
                    return NotFound(new { message = "Uçak bulunamadı" });

                return Ok(aircraft);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving aircraft {AircraftId}", id);
                return StatusCode(500, new { message = "Uçak getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Kayıt numarasına göre uçak getirir
        /// </summary>
        [HttpGet("registration/{registrationNumber}")]
        [ProducesResponseType(typeof(AircraftResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAircraftByRegistrationNumber(string registrationNumber)
        {
            try
            {
                var aircraft = await _aircraftService.GetAircraftByRegistrationNumberAsync(registrationNumber);
                if (aircraft == null)
                    return NotFound(new { message = "Uçak bulunamadı" });

                return Ok(aircraft);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving aircraft by registration {RegistrationNumber}", registrationNumber);
                return StatusCode(500, new { message = "Uçak getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Uçak tipine göre uçakları getirir
        /// </summary>
        [HttpGet("type/{aircraftType}")]
        [ProducesResponseType(typeof(IEnumerable<AircraftResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAircraftsByType(string aircraftType)
        {
            try
            {
                var aircrafts = await _aircraftService.GetAircraftsByTypeAsync(aircraftType);
                return Ok(aircrafts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving aircrafts by type {AircraftType}", aircraftType);
                return StatusCode(500, new { message = "Uçaklar getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Yeni uçak oluşturur
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(AircraftResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAircraft([FromBody] CreateAircraftDto createDto)
        {
            try
            {
                var aircraft = await _aircraftService.CreateAircraftAsync(createDto);
                return CreatedAtAction(nameof(GetAircraftById), new { id = aircraft.AircraftId }, aircraft);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating aircraft");
                return StatusCode(500, new { message = "Uçak oluşturulurken hata oluştu" });
            }
        }

        /// <summary>
        /// Uçak bilgilerini günceller
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(AircraftResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAircraft(int id, [FromBody] UpdateAircraftDto updateDto)
        {
            try
            {
                var aircraft = await _aircraftService.UpdateAircraftAsync(id, updateDto);
                return Ok(aircraft);
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
                _logger.LogError(ex, "Error updating aircraft {AircraftId}", id);
                return StatusCode(500, new { message = "Uçak güncellenirken hata oluştu" });
            }
        }

        /// <summary>
        /// Uçağı siler
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAircraft(int id)
        {
            try
            {
                await _aircraftService.DeleteAircraftAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting aircraft {AircraftId}", id);
                return StatusCode(500, new { message = "Uçak silinirken hata oluştu" });
            }
        }

        /// <summary>
        /// Uçağı aktif eder
        /// </summary>
        [HttpPatch("{id}/activate")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateAircraft(int id)
        {
            try
            {
                await _aircraftService.ActivateAircraftAsync(id);
                return Ok(new { message = "Uçak aktif edildi" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating aircraft {AircraftId}", id);
                return StatusCode(500, new { message = "Uçak aktif edilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Uçağı pasif eder
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateAircraft(int id)
        {
            try
            {
                await _aircraftService.DeactivateAircraftAsync(id);
                return Ok(new { message = "Uçak pasif edildi" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating aircraft {AircraftId}", id);
                return StatusCode(500, new { message = "Uçak pasif edilirken hata oluştu" });
            }
        }
    }
}