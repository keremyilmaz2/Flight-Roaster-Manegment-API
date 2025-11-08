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

        /// <summary>
        /// Uçuş ID'sine göre tam roster bilgisini getirir
        /// </summary>
        [HttpGet("flight/{flightId}")]
        [ProducesResponseType(typeof(FlightRosterResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFlightRoster(int flightId)
        {
            try
            {
                var roster = await _rosterService.GetFlightRosterAsync(flightId);
                if (roster == null)
                    return NotFound(new { message = "Uçuş bulunamadı" });

                return Ok(roster);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flight roster for flight {FlightId}", flightId);
                return StatusCode(500, new { message = "Roster bilgisi alınırken hata oluştu" });
            }
        }

        /// <summary>
        /// Uçuş numarasına göre roster bilgisini getirir
        /// </summary>
        [HttpGet("flight-number/{flightNumber}")]
        [ProducesResponseType(typeof(FlightRosterResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFlightRosterByNumber(string flightNumber)
        {
            try
            {
                var roster = await _rosterService.GetFlightRosterByFlightNumberAsync(flightNumber);
                if (roster == null)
                    return NotFound(new { message = "Uçuş bulunamadı" });

                return Ok(roster);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flight roster for flight number {FlightNumber}", flightNumber);
                return StatusCode(500, new { message = "Roster bilgisi alınırken hata oluştu" });
            }
        }

        /// <summary>
        /// Uçuşa manuel olarak pilot atar
        /// </summary>
        [HttpPost("assign-pilot")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(FlightCrewResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignPilot([FromBody] AssignFlightCrewDto assignDto)
        {
            try
            {
                var result = await _rosterService.AssignPilotToFlightAsync(assignDto);
                return CreatedAtAction(nameof(GetFlightRoster), new { flightId = assignDto.FlightId }, result);
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
                _logger.LogError(ex, "Error assigning pilot to flight");
                return StatusCode(500, new { message = "Pilot atama sırasında hata oluştu" });
            }
        }

        /// <summary>
        /// Uçuşa manuel olarak kabin ekibi atar
        /// </summary>
        [HttpPost("assign-cabin-crew")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(FlightCabinCrewResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignCabinCrew([FromBody] AssignCabinCrewDto assignDto)
        {
            try
            {
                var result = await _rosterService.AssignCabinCrewToFlightAsync(assignDto);
                return CreatedAtAction(nameof(GetFlightRoster), new { flightId = assignDto.FlightId }, result);
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
                _logger.LogError(ex, "Error assigning cabin crew to flight");
                return StatusCode(500, new { message = "Kabin ekibi atama sırasında hata oluştu" });
            }
        }

        /// <summary>
        /// Uçuştan pilot çıkarır
        /// </summary>
        [HttpDelete("remove-pilot/{flightId}/{pilotId}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemovePilot(int flightId, int pilotId)
        {
            try
            {
                var result = await _rosterService.RemovePilotFromFlightAsync(flightId, pilotId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing pilot from flight");
                return StatusCode(500, new { message = "Pilot çıkarma sırasında hata oluştu" });
            }
        }

        /// <summary>
        /// Uçuştan kabin ekibi çıkarır
        /// </summary>
        [HttpDelete("remove-cabin-crew/{flightId}/{cabinCrewId}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveCabinCrew(int flightId, int cabinCrewId)
        {
            try
            {
                var result = await _rosterService.RemoveCabinCrewFromFlightAsync(flightId, cabinCrewId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cabin crew from flight");
                return StatusCode(500, new { message = "Kabin ekibi çıkarma sırasında hata oluştu" });
            }
        }

        /// <summary>
        /// Uçuş ekibini otomatik atar (pilot ve/veya kabin ekibi)
        /// </summary>
        [HttpPost("auto-assign")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(FlightRosterResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AutoAssignCrew([FromBody] AutoAssignCrewDto autoAssignDto)
        {
            try
            {
                var result = await _rosterService.AutoAssignCrewAsync(autoAssignDto);
                return Ok(result);
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
                _logger.LogError(ex, "Error during auto-assignment");
                return StatusCode(500, new { message = "Otomatik atama sırasında hata oluştu" });
            }
        }

        /// <summary>
        /// Uçuş ekibinin geçerliliğini kontrol eder
        /// </summary>
        [HttpGet("validate/{flightId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateFlightCrew(int flightId)
        {
            try
            {
                var isValid = await _rosterService.ValidateFlightCrewAsync(flightId);
                return Ok(new { isValid, message = isValid ? "Ekip geçerli" : "Ekip geçersiz" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating flight crew");
                return StatusCode(500, new { message = "Doğrulama sırasında hata oluştu" });
            }
        }

        /// <summary>
        /// Roster bilgisini JSON formatında dışa aktarır
        /// </summary>
        [HttpGet("export/json/{flightId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportToJson(int flightId)
        {
            try
            {
                var json = await _rosterService.ExportRosterToJsonAsync(flightId);
                return Content(json, "application/json");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting roster to JSON");
                return StatusCode(500, new { message = "JSON dışa aktarma sırasında hata oluştu" });
            }
        }

        /// <summary>
        /// Roster bilgisini PDF formatında dışa aktarır
        /// </summary>
        [HttpGet("export/pdf/{flightId}")]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public async Task<IActionResult> ExportToPdf(int flightId)
        {
            try
            {
                var pdf = await _rosterService.ExportRosterToPdfAsync(flightId);
                return File(pdf, "application/pdf", $"roster-{flightId}.pdf");
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, new { message = "PDF dışa aktarma özelliği henüz eklenmedi" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting roster to PDF");
                return StatusCode(500, new { message = "PDF dışa aktarma sırasında hata oluştu" });
            }
        }
    }
}