using FlightRosterAPI.Models.DTOs.Flight;
using FlightRosterAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightRosterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightService _flightService;
        private readonly ILogger<FlightsController> _logger;

        public FlightsController(
            IFlightService flightService,
            ILogger<FlightsController> logger)
        {
            _flightService = flightService;
            _logger = logger;
        }

        /// <summary>
        /// Tüm uçuşları getirir
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FlightResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllFlights()
        {
            try
            {
                var flights = await _flightService.GetAllFlightsAsync();
                return Ok(flights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all flights");
                return StatusCode(500, new { message = "Uçuşlar getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Aktif uçuşları getirir
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<FlightResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveFlights()
        {
            try
            {
                var flights = await _flightService.GetActiveFlightsAsync();
                return Ok(flights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active flights");
                return StatusCode(500, new { message = "Aktif uçuşlar getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// ID'ye göre uçuş getirir
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FlightResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFlightById(int id)
        {
            try
            {
                var flight = await _flightService.GetFlightByIdAsync(id);
                if (flight == null)
                    return NotFound(new { message = "Uçuş bulunamadı" });

                return Ok(flight);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flight {FlightId}", id);
                return StatusCode(500, new { message = "Uçuş getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Detaylı uçuş bilgisini getirir (ekip ve yolcularla birlikte)
        /// </summary>
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(FlightDetailResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFlightWithDetails(int id)
        {
            try
            {
                var flight = await _flightService.GetFlightWithDetailsAsync(id);
                if (flight == null)
                    return NotFound(new { message = "Uçuş bulunamadı" });

                return Ok(flight);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flight details {FlightId}", id);
                return StatusCode(500, new { message = "Uçuş detayları getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Uçuş numarasına göre uçuş getirir
        /// </summary>
        [HttpGet("number/{flightNumber}")]
        [ProducesResponseType(typeof(FlightResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFlightByNumber(string flightNumber)
        {
            try
            {
                var flight = await _flightService.GetFlightByFlightNumberAsync(flightNumber);
                if (flight == null)
                    return NotFound(new { message = "Uçuş bulunamadı" });

                return Ok(flight);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flight by number {FlightNumber}", flightNumber);
                return StatusCode(500, new { message = "Uçuş getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Tarih aralığına göre uçuşları getirir
        /// </summary>
        [HttpGet("date-range")]
        [ProducesResponseType(typeof(IEnumerable<FlightResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFlightsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var flights = await _flightService.GetFlightsByDateRangeAsync(startDate, endDate);
                return Ok(flights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flights by date range");
                return StatusCode(500, new { message = "Uçuşlar getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Uçağa göre uçuşları getirir
        /// </summary>
        [HttpGet("aircraft/{aircraftId}")]
        [ProducesResponseType(typeof(IEnumerable<FlightResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFlightsByAircraft(int aircraftId)
        {
            try
            {
                var flights = await _flightService.GetFlightsByAircraftAsync(aircraftId);
                return Ok(flights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flights by aircraft {AircraftId}", aircraftId);
                return StatusCode(500, new { message = "Uçuşlar getirilirken hata oluştu" });
            }
        }

        /// <summary>
        /// Uçuş arama (uçuş numarası, kalkış, varış)
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<FlightResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchFlights(
            [FromQuery] string? flightNumber,
            [FromQuery] string? departure,
            [FromQuery] string? arrival)
        {
            try
            {
                var flights = await _flightService.SearchFlightsAsync(flightNumber, departure, arrival);
                return Ok(flights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching flights");
                return StatusCode(500, new { message = "Uçuş arama sırasında hata oluştu" });
            }
        }

        /// <summary>
        /// Yeni uçuş oluşturur
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(FlightResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateFlight([FromBody] CreateFlightDto createDto)
        {
            try
            {
                var flight = await _flightService.CreateFlightAsync(createDto);
                return CreatedAtAction(nameof(GetFlightById), new { id = flight.FlightId }, flight);
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
                _logger.LogError(ex, "Error creating flight");
                return StatusCode(500, new { message = "Uçuş oluşturulurken hata oluştu" });
            }
        }

        /// <summary>
        /// Uçuş bilgilerini günceller
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(FlightResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateFlight(int id, [FromBody] UpdateFlightDto updateDto)
        {
            try
            {
                var flight = await _flightService.UpdateFlightAsync(id, updateDto);
                return Ok(flight);
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
                _logger.LogError(ex, "Error updating flight {FlightId}", id);
                return StatusCode(500, new { message = "Uçuş güncellenirken hata oluştu" });
            }
        }

        /// <summary>
        /// Uçuşu siler
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFlight(int id)
        {
            try
            {
                await _flightService.DeleteFlightAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting flight {FlightId}", id);
                return StatusCode(500, new { message = "Uçuş silinirken hata oluştu" });
            }
        }
    }
}