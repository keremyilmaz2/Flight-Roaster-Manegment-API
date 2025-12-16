using FlightRosterAPI.Models;
using FlightRosterAPI.Models.DTOs.CabinCrew;
using FlightRosterAPI.Models.Enums;
using FlightRosterAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightRosterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CabinCrewController : ControllerBase
    {
        private readonly ICabinCrewService _cabinCrewService;
        private readonly ILogger<CabinCrewController> _logger;

        public CabinCrewController(
            ICabinCrewService cabinCrewService,
            ILogger<CabinCrewController> logger)
        {
            _cabinCrewService = cabinCrewService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCabinCrew()
        {
            var response = new ResponseDto();
            try
            {
                var cabinCrew = await _cabinCrewService.GetAllCabinCrewAsync();
                response.Result = cabinCrew;
                response.Message = "Kabin ekibi başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all cabin crew");
                response.IsSuccess = false;
                response.Message = "Kabin ekibi getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCabinCrew()
        {
            var response = new ResponseDto();
            try
            {
                var cabinCrew = await _cabinCrewService.GetActiveCabinCrewAsync();
                response.Result = cabinCrew;
                response.Message = "Aktif kabin ekibi başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active cabin crew");
                response.IsSuccess = false;
                response.Message = "Aktif kabin ekibi getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCabinCrewById(int id)
        {
            var response = new ResponseDto();
            try
            {
                var cabinCrew = await _cabinCrewService.GetCabinCrewByIdAsync(id);
                if (cabinCrew == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Kabin ekibi bulunamadı";
                    return NotFound(response);
                }

                response.Result = cabinCrew;
                response.Message = "Kabin ekibi başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cabin crew {CabinCrewId}", id);
                response.IsSuccess = false;
                response.Message = "Kabin ekibi getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCabinCrewByUserId(int userId)
        {
            var response = new ResponseDto();
            try
            {
                var cabinCrew = await _cabinCrewService.GetCabinCrewByUserIdAsync(userId);
                if (cabinCrew == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Kabin ekibi bulunamadı";
                    return NotFound(response);
                }

                response.Result = cabinCrew;
                response.Message = "Kabin ekibi başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cabin crew by user {UserId}", userId);
                response.IsSuccess = false;
                response.Message = "Kabin ekibi getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("type/{crewType}")]
        public async Task<IActionResult> GetCabinCrewByType(CabinCrewType crewType)
        {
            var response = new ResponseDto();
            try
            {
                var cabinCrew = await _cabinCrewService.GetCabinCrewByTypeAsync(crewType);
                response.Result = cabinCrew;
                response.Message = "Kabin ekibi başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cabin crew by type {CrewType}", crewType);
                response.IsSuccess = false;
                response.Message = "Kabin ekibi getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("seniority/{seniority}")]
        public async Task<IActionResult> GetCabinCrewBySeniority(CabinCrewSeniority seniority)
        {
            var response = new ResponseDto();
            try
            {
                var cabinCrew = await _cabinCrewService.GetCabinCrewBySeniorityAsync(seniority);
                response.Result = cabinCrew;
                response.Message = "Kabin ekibi başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cabin crew by seniority {Seniority}", seniority);
                response.IsSuccess = false;
                response.Message = "Kabin ekibi getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("available-for-flight/{flightId}")]
        public async Task<IActionResult> GetAvailableCabinCrewForFlight(int flightId)
        {
            var response = new ResponseDto();
            try
            {
                var cabinCrew = await _cabinCrewService.GetAvailableCabinCrewForFlightAsync(flightId);
                response.Result = cabinCrew;
                response.Message = "Uygun kabin ekibi başarıyla getirildi";
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
                _logger.LogError(ex, "Error retrieving available cabin crew for flight {FlightId}", flightId);
                response.IsSuccess = false;
                response.Message = "Uygun kabin ekibi getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("chefs-with-recipes")]
        public async Task<IActionResult> GetChefsWithRecipes()
        {
            var response = new ResponseDto();
            try
            {
                var chefs = await _cabinCrewService.GetChefsWithRecipesAsync();
                response.Result = chefs;
                response.Message = "Aşçılar başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chefs with recipes");
                response.IsSuccess = false;
                response.Message = "Aşçılar getirilirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateCabinCrew([FromBody] CreateCabinCrewDto createDto)
        {
            var response = new ResponseDto();
            try
            {
                var cabinCrew = await _cabinCrewService.CreateCabinCrewAsync(createDto);
                response.Result = cabinCrew;
                response.Message = "Kabin ekibi başarıyla oluşturuldu";
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
                _logger.LogError(ex, "Error creating cabin crew");
                response.IsSuccess = false;
                response.Message = "Kabin ekibi oluşturulurken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateCabinCrew(int id, [FromBody] UpdateCabinCrewDto updateDto)
        {
            var response = new ResponseDto();
            try
            {
                var cabinCrew = await _cabinCrewService.UpdateCabinCrewAsync(id, updateDto);
                response.Result = cabinCrew;
                response.Message = "Kabin ekibi başarıyla güncellendi";
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
                _logger.LogError(ex, "Error updating cabin crew {CabinCrewId}", id);
                response.IsSuccess = false;
                response.Message = "Kabin ekibi güncellenirken hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteCabinCrew(int id)
        {
            var response = new ResponseDto();
            try
            {
                await _cabinCrewService.DeleteCabinCrewAsync(id);
                response.Message = "Kabin ekibi başarıyla silindi";
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
                _logger.LogError(ex, "Error deleting cabin crew {CabinCrewId}", id);
                response.IsSuccess = false;
                response.Message = "Kabin ekibi silinirken hata oluştu";
                return StatusCode(500, response);
            }
        }
    }
}