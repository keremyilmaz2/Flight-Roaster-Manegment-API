using FlightRosterAPI.Models;
using FlightRosterAPI.Models.DTOs.Passenger;
using FlightRosterAPI.Models.DTOs.Seat;
using FlightRosterAPI.Models.Enums;
using FlightRosterAPI.Services;
using FlightRosterAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            var response = new ResponseDto();
            try
            {
                var passengers = await _passengerService.GetAllPassengersAsync();
                response.Result = passengers;
                response.Message = "Yolcular başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all passengers");
                response.IsSuccess = false;
                response.Message = "Yolcular getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActivePassengers()
        {
            var response = new ResponseDto();
            try
            {
                var passengers = await _passengerService.GetActivePassengersAsync();
                response.Result = passengers;
                response.Message = "Aktif yolcular başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active passengers");
                response.IsSuccess = false;
                response.Message = "Aktif yolcular getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPassengerById(int id)
        {
            var response = new ResponseDto();
            try
            {
                var passenger = await _passengerService.GetPassengerByIdAsync(id);
                if (passenger == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Yolcu bulunamadı";
                    return NotFound(response);
                }

                response.Result = passenger;
                response.Message = "Yolcu başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving passenger {PassengerId}", id);
                response.IsSuccess = false;
                response.Message = "Yolcu getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPassengerByUserId(int userId)
        {
            var response = new ResponseDto();
            try
            {
                var passenger = await _passengerService.GetPassengerByUserIdAsync(userId);
                if (passenger == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Yolcu bulunamadı";
                    return NotFound(response);
                }

                response.Result = passenger;
                response.Message = "Yolcu başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving passenger by user {UserId}", userId);
                response.IsSuccess = false;
                response.Message = "Yolcu getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("flight/{flightId}")]
        public async Task<IActionResult> GetPassengersByFlight(int flightId)
        {
            var response = new ResponseDto();
            try
            {
                var passengers = await _passengerService.GetPassengersByFlightAsync(flightId);
                response.Result = passengers;
                response.Message = "Yolcular başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving passengers for flight {FlightId}", flightId);
                response.IsSuccess = false;
                response.Message = "Yolcular getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreatePassenger([FromBody] CreatePassengerDto createDto)
        {
            var response = new ResponseDto();
            try
            {
                var passenger = await _passengerService.CreatePassengerAsync(createDto);
                response.Result = passenger;
                response.Message = "Yolcu başarıyla oluşturuldu";
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
                _logger.LogError(ex, "Error creating passenger");
                response.IsSuccess = false;
                response.Message = "Yolcu oluşturulurken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePassenger(int id, [FromBody] UpdatePassengerDto updateDto)
        {
            var response = new ResponseDto();
            try
            {
                var passenger = await _passengerService.UpdatePassengerAsync(id, updateDto);
                response.Result = passenger;
                response.Message = "Yolcu başarıyla güncellendi";
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
                _logger.LogError(ex, "Error updating passenger {PassengerId}", id);
                response.IsSuccess = false;
                response.Message = "Yolcu güncellenirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeletePassenger(int id)
        {
            var response = new ResponseDto();
            try
            {
                await _passengerService.DeletePassengerAsync(id);
                response.Message = "Yolcu başarıyla silindi";
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
                _logger.LogError(ex, "Error deleting passenger {PassengerId}", id);
                response.IsSuccess = false;
                response.Message = "Yolcu silinirken hata oluştu";
                return StatusCode(500, response);
            }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SeatsController : ControllerBase
    {
        private readonly ISeatService _seatService;
        private readonly IPassengerService _passengerService;
        private readonly ILogger<SeatsController> _logger;

        public SeatsController(
            ISeatService seatService,
            IPassengerService passengerService,
            ILogger<SeatsController> logger)
        {
            _seatService = seatService;
            _passengerService = passengerService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSeatById(int id)
        {
            var response = new ResponseDto();
            try
            {
                var seat = await _seatService.GetSeatByIdAsync(id);
                if (seat == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Koltuk bulunamadı";
                    return NotFound(response);
                }

                response.Result = seat;
                response.Message = "Koltuk başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving seat {SeatId}", id);
                response.IsSuccess = false;
                response.Message = "Koltuk getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("flight/{flightId}/seat-map")]
        public async Task<IActionResult> GetSeatMapByFlight(int flightId)
        {
            var response = new ResponseDto();
            try
            {
                var seatMap = await _seatService.GetSeatMapByFlightAsync(flightId);
                if (seatMap == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Uçuş bulunamadı";
                    return NotFound(response);
                }

                response.Result = seatMap;
                response.Message = "Koltuk planı başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving seat map for flight {FlightId}", flightId);
                response.IsSuccess = false;
                response.Message = "Koltuk planı getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("flight/{flightId}/available")]
        public async Task<IActionResult> GetAvailableSeatsByFlight(int flightId)
        {
            var response = new ResponseDto();
            try
            {
                var seats = await _seatService.GetAvailableSeatsByFlightAsync(flightId);
                response.Result = seats;
                response.Message = "Müsait koltuklar başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available seats for flight {FlightId}", flightId);
                response.IsSuccess = false;
                response.Message = "Müsait koltuklar getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("flight/{flightId}/class/{seatClass}")]
        public async Task<IActionResult> GetSeatsByClass(int flightId, SeatClass seatClass)
        {
            var response = new ResponseDto();
            try
            {
                var seats = await _seatService.GetSeatsByClassAsync(flightId, seatClass);
                response.Result = seats;
                response.Message = "Koltuklar başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving seats by class for flight {FlightId}", flightId);
                response.IsSuccess = false;
                response.Message = "Koltuklar getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateSeat([FromBody] CreateSeatDto createDto)
        {
            var response = new ResponseDto();
            try
            {
                var seat = await _seatService.CreateSeatAsync(createDto);
                response.Result = seat;
                response.Message = "Koltuk başarıyla oluşturuldu";
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
                _logger.LogError(ex, "Error creating seat");
                response.IsSuccess = false;
                response.Message = "Koltuk oluşturulurken hata oluştu";
                return StatusCode(500, response);
            }
        }

        // Controllers/SeatsController.cs

        [HttpGet("my-seats")]
        public async Task<IActionResult> GetMySeats([FromQuery] bool includeCompleted = true)
        {
            var response = new ResponseDto();
            try
            {
                var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Kullanıcı kimliği bulunamadı";
                    return Unauthorized(response);
                }

                int userId = int.Parse(userIdClaim.Value);
                var passenger = await _passengerService.GetPassengerByUserIdAsync(userId);

                if (passenger == null)
                {
                    response.Result = new List<SeatResponseDto>();
                    response.Message = "Henüz yolcu kaydınız yok";
                    return Ok(response);
                }

                var seats = await _seatService.GetSeatsByPassengerAsync(passenger.PassengerId);

                // ✅ Tamamlanmış uçuşları filtrele (isteğe bağlı)
                if (!includeCompleted)
                {
                    var now = DateTime.UtcNow;
                    seats = seats.Where(s => s.FlightInfo != null && s.FlightInfo.ArrivalTime > now);
                }

                response.Result = seats;
                response.Message = includeCompleted
                    ? "Tüm rezervasyonlarınız başarıyla getirildi"
                    : "Aktif rezervasyonlarınız başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user's seats");
                response.IsSuccess = false;
                response.Message = "Rezervasyonlar getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        // ✅ Bonus: Rezervasyon Özeti
        [HttpGet("my-seats-summary")]
        public async Task<IActionResult> GetMySeatsSummary()
        {
            var response = new ResponseDto();
            try
            {
                var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Kullanıcı kimliği bulunamadı";
                    return Unauthorized(response);
                }

                int userId = int.Parse(userIdClaim.Value);
                var passenger = await _passengerService.GetPassengerByUserIdAsync(userId);

                if (passenger == null)
                {
                    response.Result = new { Upcoming = new List<SeatResponseDto>(), Completed = new List<SeatResponseDto>() };
                    return Ok(response);
                }

                var allSeats = await _seatService.GetSeatsByPassengerAsync(passenger.PassengerId);
                var now = DateTime.UtcNow;

                var upcoming = allSeats
                    .Where(s => s.FlightInfo != null && s.FlightInfo.ArrivalTime > now)
                    .OrderBy(s => s.FlightInfo!.DepartureTime)
                    .ToList();

                var completed = allSeats
                    .Where(s => s.FlightInfo != null && s.FlightInfo.ArrivalTime <= now)
                    .OrderByDescending(s => s.FlightInfo!.DepartureTime)
                    .ToList();

                response.Result = new
                {
                    Upcoming = upcoming,
                    Completed = completed,
                    Statistics = new
                    {
                        TotalFlights = allSeats.Count(),
                        UpcomingFlights = upcoming.Count,
                        CompletedFlights = completed.Count,
                        TotalDistanceKm = allSeats.Sum(s => s.FlightInfo?.DistanceKm ?? 0),
                        TotalFlightTimeMinutes = allSeats.Sum(s => s.FlightInfo?.DurationMinutes ?? 0),
                        BusinessClassSeats = allSeats.Count(s => s.SeatClass == SeatClass.Business),
                        EconomyClassSeats = allSeats.Count(s => s.SeatClass == SeatClass.Economy)
                    }
                };

                response.Message = "Rezervasyon özeti başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving seats summary");
                response.IsSuccess = false;
                response.Message = "Rezervasyon özeti getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }


        [HttpPost("{id}/book")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> BookSeat(int id, [FromBody] BookSeatDto bookDto)
        {
            var response = new ResponseDto();
            try
            {
                var seat = await _seatService.BookSeatAsync(id, bookDto);
                response.Result = seat;
                response.Message = "Koltuk başarıyla rezerve edildi";
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
                _logger.LogError(ex, "Error booking seat {SeatId}", id);
                response.IsSuccess = false;
                response.Message = "Koltuk rezerve edilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPost("flight/{flightId}/generate")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GenerateSeatsForFlight(int flightId)
        {
            var response = new ResponseDto();
            try
            {
                await _seatService.GenerateSeatsForFlightAsync(flightId);
                response.Message = "Koltuklar başarıyla oluşturuldu";
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
                _logger.LogError(ex, "Error generating seats for flight {FlightId}", flightId);
                response.IsSuccess = false;
                response.Message = "Koltuklar oluşturulurken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPost("flight/{flightId}/auto-assign")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AutoAssignSeats(int flightId)
        {
            var response = new ResponseDto();
            try
            {
                await _seatService.AutoAssignSeatsAsync(flightId);
                response.Message = "Koltuklar otomatik olarak atandı";
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
                _logger.LogError(ex, "Error auto-assigning seats for flight {FlightId}", flightId);
                response.IsSuccess = false;
                response.Message = "Otomatik koltuk ataması sırasında hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPost("{id}/book-self")]
        [Authorize] // Sadece giriş yapmış kullanıcılar
        public async Task<IActionResult> BookSeatForSelf(int id, [FromBody] BookSeatForSelfDto bookDto)
        {
            var response = new ResponseDto();
            try
            {
                // JWT token'dan kullanıcı ID'sini al
                var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Kullanıcı kimliği bulunamadı";
                    return Unauthorized(response);
                }

                int userId = int.Parse(userIdClaim.Value);

                // Kullanıcının passenger kaydını bul
                var passenger = await _passengerService.GetPassengerByUserIdAsync(userId);
                if (passenger == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Yolcu kaydınız bulunamadı";
                    return NotFound(response);
                }

                // Koltuk rezervasyonu yap
                var bookSeatDto = new BookSeatDto
                {
                    PassengerId = passenger.PassengerId,
                    IsInfantSeat = bookDto.IsInfantSeat,
                    ParentPassengerId = bookDto.ParentPassengerId
                };

                var seat = await _seatService.BookSeatAsync(id, bookSeatDto);
                response.Result = seat;
                response.Message = "Koltuğunuz başarıyla rezerve edildi";
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
                _logger.LogError(ex, "Error booking seat {SeatId} for self", id);
                response.IsSuccess = false;
                response.Message = "Koltuk rezerve edilirken hata oluştu";
                return StatusCode(500, response);
            }
        }


        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateSeat(int id, [FromBody] UpdateSeatDto updateDto)
        {
            var response = new ResponseDto();
            try
            {
                var seat = await _seatService.UpdateSeatAsync(id, updateDto);
                response.Result = seat;
                response.Message = "Koltuk başarıyla güncellendi";
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
                _logger.LogError(ex, "Error updating seat {SeatId}", id);
                response.IsSuccess = false;
                response.Message = "Koltuk güncellenirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteSeat(int id)
        {
            var response = new ResponseDto();
            try
            {
                await _seatService.DeleteSeatAsync(id);
                response.Message = "Koltuk başarıyla silindi";
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
                _logger.LogError(ex, "Error deleting seat {SeatId}", id);
                response.IsSuccess = false;
                response.Message = "Koltuk silinirken hata oluştu";
                return StatusCode(500, response);
            }
        }
    }
}