using FlightRosterAPI.Models;
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

        [HttpGet]
        public async Task<IActionResult> GetAllFlights()
        {
            var response = new ResponseDto();
            try
            {
                var flights = await _flightService.GetAllFlightsAsync();
                response.Result = flights;
                response.Message = "Uçuşlar başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all flights");
                response.IsSuccess = false;
                response.Message = "Uçuşlar getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveFlights()
        {
            var response = new ResponseDto();
            try
            {
                var flights = await _flightService.GetActiveFlightsAsync();
                response.Result = flights;
                response.Message = "Aktif uçuşlar başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active flights");
                response.IsSuccess = false;
                response.Message = "Aktif uçuşlar getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlightById(int id)
        {
            var response = new ResponseDto();
            try
            {
                var flight = await _flightService.GetFlightByIdAsync(id);
                if (flight == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Uçuş bulunamadı";
                    return NotFound(response);
                }

                response.Result = flight;
                response.Message = "Uçuş başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flight {FlightId}", id);
                response.IsSuccess = false;
                response.Message = "Uçuş getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetFlightWithDetails(int id)
        {
            var response = new ResponseDto();
            try
            {
                var flight = await _flightService.GetFlightWithDetailsAsync(id);
                if (flight == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Uçuş bulunamadı";
                    return NotFound(response);
                }

                response.Result = flight;
                response.Message = "Uçuş detayları başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flight details {FlightId}", id);
                response.IsSuccess = false;
                response.Message = "Uçuş detayları getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("number/{flightNumber}")]
        public async Task<IActionResult> GetFlightByNumber(string flightNumber)
        {
            var response = new ResponseDto();
            try
            {
                var flight = await _flightService.GetFlightByFlightNumberAsync(flightNumber);
                if (flight == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Uçuş bulunamadı";
                    return NotFound(response);
                }

                response.Result = flight;
                response.Message = "Uçuş başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flight by number {FlightNumber}", flightNumber);
                response.IsSuccess = false;
                response.Message = "Uçuş getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("date-range")]
        public async Task<IActionResult> GetFlightsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var response = new ResponseDto();
            try
            {
                var flights = await _flightService.GetFlightsByDateRangeAsync(startDate, endDate);
                response.Result = flights;
                response.Message = "Uçuşlar başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flights by date range");
                response.IsSuccess = false;
                response.Message = "Uçuşlar getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("aircraft/{aircraftId}")]
        public async Task<IActionResult> GetFlightsByAircraft(int aircraftId)
        {
            var response = new ResponseDto();
            try
            {
                var flights = await _flightService.GetFlightsByAircraftAsync(aircraftId);
                response.Result = flights;
                response.Message = "Uçuşlar başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flights by aircraft {AircraftId}", aircraftId);
                response.IsSuccess = false;
                response.Message = "Uçuşlar getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchFlights(
            [FromQuery] string? flightNumber,
            [FromQuery] string? departure,
            [FromQuery] string? arrival)
        {
            var response = new ResponseDto();
            try
            {
                var flights = await _flightService.SearchFlightsAsync(flightNumber, departure, arrival);
                response.Result = flights;
                response.Message = "Uçuş araması başarıyla tamamlandı";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching flights");
                response.IsSuccess = false;
                response.Message = "Uçuş arama sırasında hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateFlight([FromBody] CreateFlightDto createDto)
        {
            var response = new ResponseDto();
            try
            {
                var flight = await _flightService.CreateFlightAsync(createDto);
                response.Result = flight;
                response.Message = "Uçuş başarıyla oluşturuldu";
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
                _logger.LogError(ex, "Error creating flight");
                response.IsSuccess = false;
                response.Message = "Uçuş oluşturulurken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateFlight(int id, [FromBody] UpdateFlightDto updateDto)
        {
            var response = new ResponseDto();
            try
            {
                var flight = await _flightService.UpdateFlightAsync(id, updateDto);
                response.Result = flight;
                response.Message = "Uçuş başarıyla güncellendi";
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
                _logger.LogError(ex, "Error updating flight {FlightId}", id);
                response.IsSuccess = false;
                response.Message = "Uçuş güncellenirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteFlight(int id)
        {
            var response = new ResponseDto();
            try
            {
                await _flightService.DeleteFlightAsync(id);
                response.Message = "Uçuş başarıyla silindi";
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
                _logger.LogError(ex, "Error deleting flight {FlightId}", id);
                response.IsSuccess = false;
                response.Message = "Uçuş silinirken hata oluştu";
                return StatusCode(500, response);
            }
        }
    }
}