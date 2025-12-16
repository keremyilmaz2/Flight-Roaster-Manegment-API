using FlightRosterAPI.Models;
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

        [HttpGet]
        public async Task<IActionResult> GetAllPilots()
        {
            var response = new ResponseDto();
            try
            {
                var pilots = await _pilotService.GetAllPilotsAsync();
                response.Result = pilots;
                response.Message = "Pilotlar başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all pilots");
                response.IsSuccess = false;
                response.Message = "Pilotlar getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActivePilots()
        {
            var response = new ResponseDto();
            try
            {
                var pilots = await _pilotService.GetActivePilotsAsync();
                response.Result = pilots;
                response.Message = "Aktif pilotlar başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active pilots");
                response.IsSuccess = false;
                response.Message = "Aktif pilotlar getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPilotById(int id)
        {
            var response = new ResponseDto();
            try
            {
                var pilot = await _pilotService.GetPilotByIdAsync(id);
                if (pilot == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Pilot bulunamadı";
                    return NotFound(response);
                }

                response.Result = pilot;
                response.Message = "Pilot başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pilot {PilotId}", id);
                response.IsSuccess = false;
                response.Message = "Pilot getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPilotByUserId(int userId)
        {
            var response = new ResponseDto();
            try
            {
                var pilot = await _pilotService.GetPilotByUserIdAsync(userId);
                if (pilot == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Pilot bulunamadı";
                    return NotFound(response);
                }

                response.Result = pilot;
                response.Message = "Pilot başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pilot by user {UserId}", userId);
                response.IsSuccess = false;
                response.Message = "Pilot getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("license/{licenseNumber}")]
        public async Task<IActionResult> GetPilotByLicenseNumber(string licenseNumber)
        {
            var response = new ResponseDto();
            try
            {
                var pilot = await _pilotService.GetPilotByLicenseNumberAsync(licenseNumber);
                if (pilot == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Pilot bulunamadı";
                    return NotFound(response);
                }

                response.Result = pilot;
                response.Message = "Pilot başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pilot by license {LicenseNumber}", licenseNumber);
                response.IsSuccess = false;
                response.Message = "Pilot getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("seniority/{seniority}")]
        public async Task<IActionResult> GetPilotsBySeniority(PilotSeniority seniority)
        {
            var response = new ResponseDto();
            try
            {
                var pilots = await _pilotService.GetPilotsBySeniorityAsync(seniority);
                response.Result = pilots;
                response.Message = "Pilotlar başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pilots by seniority {Seniority}", seniority);
                response.IsSuccess = false;
                response.Message = "Pilotlar getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("available-for-flight/{flightId}")]
        public async Task<IActionResult> GetAvailablePilotsForFlight(int flightId)
        {
            var response = new ResponseDto();
            try
            {
                var pilots = await _pilotService.GetAvailablePilotsForFlightAsync(flightId);
                response.Result = pilots;
                response.Message = "Uygun pilotlar başarıyla getirildi";
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available pilots for flight {FlightId}", flightId);
                response.IsSuccess = false;
                response.Message = "Uygun pilotlar getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("expired-licenses")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetPilotsWithExpiredLicenses()
        {
            var response = new ResponseDto();
            try
            {
                var pilots = await _pilotService.GetPilotsWithExpiredLicensesAsync();
                response.Result = pilots;
                response.Message = "Lisansı dolmuş pilotlar başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pilots with expired licenses");
                response.IsSuccess = false;
                response.Message = "Lisansı dolmuş pilotlar getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreatePilot([FromBody] CreatePilotDto createDto)
        {
            var response = new ResponseDto();
            try
            {
                var pilot = await _pilotService.CreatePilotAsync(createDto);
                response.Result = pilot;
                response.Message = "Pilot başarıyla oluşturuldu";
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pilot");
                response.IsSuccess = false;
                response.Message = "Pilot oluşturulurken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdatePilot(int id, [FromBody] UpdatePilotDto updateDto)
        {
            var response = new ResponseDto();
            try
            {
                var pilot = await _pilotService.UpdatePilotAsync(id, updateDto);
                response.Result = pilot;
                response.Message = "Pilot başarıyla güncellendi";
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return NotFound(response);
            }
            catch (InvalidOperationException ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pilot {PilotId}", id);
                response.IsSuccess = false;
                response.Message = "Pilot güncellenirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeletePilot(int id)
        {
            var response = new ResponseDto();
            try
            {
                await _pilotService.DeletePilotAsync(id);
                response.Message = "Pilot başarıyla silindi";
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting pilot {PilotId}", id);
                response.IsSuccess = false;
                response.Message = "Pilot silinirken hata oluştu";
                return StatusCode(500, response);
            }
        }
    }
}