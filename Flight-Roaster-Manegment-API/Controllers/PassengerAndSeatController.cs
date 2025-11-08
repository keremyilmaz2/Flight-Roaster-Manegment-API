using FlightRosterAPI.Models.Enums;
using FlightRosterAPI.Models.DTOs.Passenger;
using FlightRosterAPI.Models.DTOs.Seat;
using FlightRosterAPI.Models.Enums;
using FlightRosterAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightRosterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PassengersController : ControllerBase
    {
        private readonly IPassengerService _passengerService;
        private readonly ILogger<PassengersController> _logger;

        public PassengersController(
            IPassengerService passengerService,
            ILogger<PassengersController> logger)
        {
            _passengerService = passengerService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPassengers()
        {
            try
            {
                var passengers = await _passengerService.GetAllPassengersAsync();
                return Ok(passengers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all passengers");
                return StatusCode(500, new { message = "Yolcular getirilirken hata oluştu" });
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActivePassengers()
        {
            try
            {
                var passengers = await _passengerService.GetActivePassengersAsync();
                return Ok(passengers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active passengers");
                return StatusCode(500, new { message = "Aktif yolcular getirilirken hata oluştu" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPassengerById(int id)
        {
            try
            {
                var passenger = await _passengerService.GetPassengerByIdAsync(id);
                if (passenger == null)
                    return NotFound(new { message = "Yolcu bulunamadı" });

                return Ok(passenger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving passenger {PassengerId}", id);
                return StatusCode(500, new { message = "Yolcu getirilirken hata oluştu" });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPassengerByUserId(int userId)
        {
            try
            {
                var passenger = await _passengerService.GetPassengerByUserIdAsync(userId);
                if (passenger == null)
                    return NotFound(new { message = "Yolcu bulunamadı" });

                return Ok(passenger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving passenger by user {UserId}", userId);
                return StatusCode(500, new { message = "Yolcu getirilirken hata oluştu" });
            }
        }

        [HttpGet("flight/{flightId}")]
        public async Task<IActionResult> GetPassengersByFlight(int flightId)
        {
            try
            {
                var passengers = await _passengerService.GetPassengersByFlightAsync(flightId);
                return Ok(passengers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving passengers for flight {FlightId}", flightId);
                return StatusCode(500, new { message = "Yolcular getirilirken hata oluştu" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreatePassenger([FromBody] CreatePassengerDto createDto)
        {
            try
            {
                var passenger = await _passengerService.CreatePassengerAsync(createDto);
                return CreatedAtAction(nameof(GetPassengerById), new { id = passenger.PassengerId }, passenger);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating passenger");
                return StatusCode(500, new { message = "Yolcu oluşturulurken hata oluştu" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdatePassenger(int id, [FromBody] UpdatePassengerDto updateDto)
        {
            try
            {
                var passenger = await _passengerService.UpdatePassengerAsync(id, updateDto);
                return Ok(passenger);
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
                _logger.LogError(ex, "Error updating passenger {PassengerId}", id);
                return StatusCode(500, new { message = "Yolcu güncellenirken hata oluştu" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeletePassenger(int id)
        {
            try
            {
                await _passengerService.DeletePassengerAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting passenger {PassengerId}", id);
                return StatusCode(500, new { message = "Yolcu silinirken hata oluştu" });
            }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SeatsController : ControllerBase
    {
        private readonly ISeatService _seatService;
        private readonly ILogger<SeatsController> _logger;

        public SeatsController(
            ISeatService seatService,
            ILogger<SeatsController> logger)
        {
            _seatService = seatService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSeatById(int id)
        {
            try
            {
                var seat = await _seatService.GetSeatByIdAsync(id);
                if (seat == null)
                    return NotFound(new { message = "Koltuk bulunamadı" });

                return Ok(seat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving seat {SeatId}", id);
                return StatusCode(500, new { message = "Koltuk getirilirken hata oluştu" });
            }
        }

        [HttpGet("flight/{flightId}/seat-map")]
        public async Task<IActionResult> GetSeatMapByFlight(int flightId)
        {
            try
            {
                var seatMap = await _seatService.GetSeatMapByFlightAsync(flightId);
                if (seatMap == null)
                    return NotFound(new { message = "Uçuş bulunamadı" });

                return Ok(seatMap);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving seat map for flight {FlightId}", flightId);
                return StatusCode(500, new { message = "Koltuk planı getirilirken hata oluştu" });
            }
        }

        [HttpGet("flight/{flightId}/available")]
        public async Task<IActionResult> GetAvailableSeatsByFlight(int flightId)
        {
            try
            {
                var seats = await _seatService.GetAvailableSeatsByFlightAsync(flightId);
                return Ok(seats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available seats for flight {FlightId}", flightId);
                return StatusCode(500, new { message = "Müsait koltuklar getirilirken hata oluştu" });
            }
        }

        [HttpGet("flight/{flightId}/class/{seatClass}")]
        public async Task<IActionResult> GetSeatsByClass(int flightId, SeatClass seatClass)
        {
            try
            {
                var seats = await _seatService.GetSeatsByClassAsync(flightId, seatClass);
                return Ok(seats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving seats by class for flight {FlightId}", flightId);
                return StatusCode(500, new { message = "Koltuklar getirilirken hata oluştu" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateSeat([FromBody] CreateSeatDto createDto)
        {
            try
            {
                var seat = await _seatService.CreateSeatAsync(createDto);
                return CreatedAtAction(nameof(GetSeatById), new { id = seat.SeatId }, seat);
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
                _logger.LogError(ex, "Error creating seat");
                return StatusCode(500, new { message = "Koltuk oluşturulurken hata oluştu" });
            }
        }

        [HttpPost("{id}/book")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> BookSeat(int id, [FromBody] BookSeatDto bookDto)
        {
            try
            {
                var seat = await _seatService.BookSeatAsync(id, bookDto);
                return Ok(seat);
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
                _logger.LogError(ex, "Error booking seat {SeatId}", id);
                return StatusCode(500, new { message = "Koltuk rezerve edilirken hata oluştu" });
            }
        }

        [HttpPost("flight/{flightId}/generate")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GenerateSeatsForFlight(int flightId)
        {
            try
            {
                await _seatService.GenerateSeatsForFlightAsync(flightId);
                return Ok(new { message = "Koltuklar başarıyla oluşturuldu" });
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
                _logger.LogError(ex, "Error generating seats for flight {FlightId}", flightId);
                return StatusCode(500, new { message = "Koltuklar oluşturulurken hata oluştu" });
            }
        }

        [HttpPost("flight/{flightId}/auto-assign")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AutoAssignSeats(int flightId)
        {
            try
            {
                await _seatService.AutoAssignSeatsAsync(flightId);
                return Ok(new { message = "Koltuklar otomatik atandı" });
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
                _logger.LogError(ex, "Error auto-assigning seats for flight {FlightId}", flightId);
                return StatusCode(500, new { message = "Otomatik koltuk ataması sırasında hata oluştu" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateSeat(int id, [FromBody] UpdateSeatDto updateDto)
        {
            try
            {
                var seat = await _seatService.UpdateSeatAsync(id, updateDto);
                return Ok(seat);
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
                _logger.LogError(ex, "Error updating seat {SeatId}", id);
                return StatusCode(500, new { message = "Koltuk güncellenirken hata oluştu" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteSeat(int id)
        {
            try
            {
                await _seatService.DeleteSeatAsync(id);
                return NoContent();
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
                _logger.LogError(ex, "Error deleting seat {SeatId}", id);
                return StatusCode(500, new { message = "Koltuk silinirken hata oluştu" });
            }
        }
    }
}