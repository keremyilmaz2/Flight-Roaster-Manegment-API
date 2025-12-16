using FlightRosterAPI.Models;
using FlightRosterAPI.Models.DTOs.Roster;
using FlightRosterAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightRosterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RosterController : ControllerBase
    {
        private readonly IRosterService _rosterService;
        private readonly ILogger<RosterController> _logger;

        public RosterController(IRosterService rosterService, ILogger<RosterController> logger)
        {
            _rosterService = rosterService;
            _logger = logger;
        }

        [HttpGet("flight/{flightId}")]
        public async Task<IActionResult> GetFlightRoster(int flightId)
        {
            var response = new ResponseDto();
            try
            {
                var roster = await _rosterService.GetFlightRosterAsync(flightId);
                if (roster == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Uçuş bulunamadı";
                    return NotFound(response);
                }

                response.Result = roster;
                response.Message = "Roster bilgisi başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flight roster for flight {FlightId}", flightId);
                response.IsSuccess = false;
                response.Message = "Roster bilgisi alınırken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("flight-number/{flightNumber}")]
        public async Task<IActionResult> GetFlightRosterByNumber(string flightNumber)
        {
            var response = new ResponseDto();
            try
            {
                var roster = await _rosterService.GetFlightRosterByFlightNumberAsync(flightNumber);
                if (roster == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Uçuş bulunamadı";
                    return NotFound(response);
                }

                response.Result = roster;
                response.Message = "Roster bilgisi başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flight roster for flight number {FlightNumber}", flightNumber);
                response.IsSuccess = false;
                response.Message = "Roster bilgisi alınırken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPost("assign-pilot")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AssignPilot([FromBody] AssignFlightCrewDto assignDto)
        {
            var response = new ResponseDto();
            try
            {
                var result = await _rosterService.AssignPilotToFlightAsync(assignDto);
                response.Result = result;
                response.Message = "Pilot başarıyla atandı";
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
                _logger.LogError(ex, "Error assigning pilot to flight");
                response.IsSuccess = false;
                response.Message = "Pilot atama sırasında hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPost("assign-cabin-crew")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AssignCabinCrew([FromBody] AssignCabinCrewDto assignDto)
        {
            var response = new ResponseDto();
            try
            {
                var result = await _rosterService.AssignCabinCrewToFlightAsync(assignDto);
                response.Result = result;
                response.Message = "Kabin ekibi başarıyla atandı";
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
                _logger.LogError(ex, "Error assigning cabin crew to flight");
                response.IsSuccess = false;
                response.Message = "Kabin ekibi atama sırasında hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpDelete("remove-pilot/{flightId}/{pilotId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> RemovePilot(int flightId, int pilotId)
        {
            var response = new ResponseDto();
            try
            {
                await _rosterService.RemovePilotFromFlightAsync(flightId, pilotId);
                response.Message = "Pilot başarıyla çıkarıldı";
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
                _logger.LogError(ex, "Error removing pilot from flight");
                response.IsSuccess = false;
                response.Message = "Pilot çıkarma sırasında hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpDelete("remove-cabin-crew/{flightId}/{cabinCrewId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> RemoveCabinCrew(int flightId, int cabinCrewId)
        {
            var response = new ResponseDto();
            try
            {
                await _rosterService.RemoveCabinCrewFromFlightAsync(flightId, cabinCrewId);
                response.Message = "Kabin ekibi başarıyla çıkarıldı";
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
                _logger.LogError(ex, "Error removing cabin crew from flight");
                response.IsSuccess = false;
                response.Message = "Kabin ekibi çıkarma sırasında hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPost("auto-assign")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AutoAssignCrew([FromBody] AutoAssignCrewDto autoAssignDto)
        {
            var response = new ResponseDto();
            try
            {
                var result = await _rosterService.AutoAssignCrewAsync(autoAssignDto);
                response.Result = result;
                response.Message = "Ekip otomatik olarak atandı";
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
                _logger.LogError(ex, "Error during auto-assignment");
                response.IsSuccess = false;
                response.Message = "Otomatik atama sırasında hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("validate/{flightId}")]
        public async Task<IActionResult> ValidateFlightCrew(int flightId)
        {
            var response = new ResponseDto();
            try
            {
                var isValid = await _rosterService.ValidateFlightCrewAsync(flightId);
                response.Result = new { isValid };
                response.Message = isValid ? "Ekip geçerli" : "Ekip geçersiz";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating flight crew");
                response.IsSuccess = false;
                response.Message = "Doğrulama sırasında hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("export/json/{flightId}")]
        public async Task<IActionResult> ExportToJson(int flightId)
        {
            var response = new ResponseDto();
            try
            {
                var json = await _rosterService.ExportRosterToJsonAsync(flightId);
                response.Result = json;
                response.Message = "JSON başarıyla oluşturuldu";
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
                _logger.LogError(ex, "Error exporting roster to JSON");
                response.IsSuccess = false;
                response.Message = "JSON dışa aktarma sırasında hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("export/pdf/{flightId}")]
        public async Task<IActionResult> ExportToPdf(int flightId)
        {
            var response = new ResponseDto();
            try
            {
                var pdf = await _rosterService.ExportRosterToPdfAsync(flightId);
                return File(pdf, "application/pdf", $"roster-{flightId}.pdf");
            }
            catch (NotImplementedException)
            {
                response.IsSuccess = false;
                response.Message = "PDF dışa aktarma özelliği henüz eklenmedi";
                return StatusCode(501, response);
            }
            catch (KeyNotFoundException ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return NotFound(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting roster to PDF");
                response.IsSuccess = false;
                response.Message = "PDF dışa aktarma sırasında hata oluştu";
                return StatusCode(500, response);
            }
        }
    }
}