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
            try
            {
                var cabinCrew = await _cabinCrewService.GetAllCabinCrewAsync();
                return Ok(cabinCrew);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all cabin crew");
                return StatusCode(500, new { message = "Kabin ekibi getirilirken hata oluştu" });
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCabinCrew()
        {
            try
            {
                var cabinCrew = await _cabinCrewService.GetActiveCabinCrewAsync();
                return Ok(cabinCrew);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active cabin crew");
                return StatusCode(500, new { message = "Aktif kabin ekibi getirilirken hata oluştu" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCabinCrewById(int id)
        {
            try
            {
                var cabinCrew = await _cabinCrewService.GetCabinCrewByIdAsync(id);
                if (cabinCrew == null)
                    return NotFound(new { message = "Kabin ekibi bulunamadı" });

                return Ok(cabinCrew);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cabin crew {CabinCrewId}", id);
                return StatusCode(500, new { message = "Kabin ekibi getirilirken hata oluştu" });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCabinCrewByUserId(int userId)
        {
            try
            {
                var cabinCrew = await _cabinCrewService.GetCabinCrewByUserIdAsync(userId);
                if (cabinCrew == null)
                    return NotFound(new { message = "Kabin ekibi bulunamadı" });

                return Ok(cabinCrew);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cabin crew by user {UserId}", userId);
                return StatusCode(500, new { message = "Kabin ekibi getirilirken hata oluştu" });
            }
        }

        [HttpGet("type/{crewType}")]
        public async Task<IActionResult> GetCabinCrewByType(CabinCrewType crewType)
        {
            try
            {
                var cabinCrew = await _cabinCrewService.GetCabinCrewByTypeAsync(crewType);
                return Ok(cabinCrew);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cabin crew by type {CrewType}", crewType);
                return StatusCode(500, new { message = "Kabin ekibi getirilirken hata oluştu" });
            }
        }

        [HttpGet("seniority/{seniority}")]
        public async Task<IActionResult> GetCabinCrewBySeniority(CabinCrewSeniority seniority)
        {
            try
            {
                var cabinCrew = await _cabinCrewService.GetCabinCrewBySeniorityAsync(seniority);
                return Ok(cabinCrew);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cabin crew by seniority {Seniority}", seniority);
                return StatusCode(500, new { message = "Kabin ekibi getirilirken hata oluştu" });
            }
        }

        [HttpGet("available-for-flight/{flightId}")]
        public async Task<IActionResult> GetAvailableCabinCrewForFlight(int flightId)
        {
            try
            {
                var cabinCrew = await _cabinCrewService.GetAvailableCabinCrewForFlightAsync(flightId);
                return Ok(cabinCrew);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available cabin crew for flight {FlightId}", flightId);
                return StatusCode(500, new { message = "Uygun kabin ekibi getirilirken hata oluştu" });
            }
        }

        [HttpGet("chefs-with-recipes")]
        public async Task<IActionResult> GetChefsWithRecipes()
        {
            try
            {
                var chefs = await _cabinCrewService.GetChefsWithRecipesAsync();
                return Ok(chefs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chefs with recipes");
                return StatusCode(500, new { message = "Aşçılar getirilirken hata oluştu" });
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateCabinCrew([FromBody] CreateCabinCrewDto createDto)
        {
            try
            {
                var cabinCrew = await _cabinCrewService.CreateCabinCrewAsync(createDto);
                return CreatedAtAction(nameof(GetCabinCrewById), new { id = cabinCrew.CabinCrewId }, cabinCrew);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cabin crew");
                return StatusCode(500, new { message = "Kabin ekibi oluşturulurken hata oluştu" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateCabinCrew(int id, [FromBody] UpdateCabinCrewDto updateDto)
        {
            try
            {
                var cabinCrew = await _cabinCrewService.UpdateCabinCrewAsync(id, updateDto);
                return Ok(cabinCrew);
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
                _logger.LogError(ex, "Error updating cabin crew {CabinCrewId}", id);
                return StatusCode(500, new { message = "Kabin ekibi güncellenirken hata oluştu" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteCabinCrew(int id)
        {
            try
            {
                await _cabinCrewService.DeleteCabinCrewAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cabin crew {CabinCrewId}", id);
                return StatusCode(500, new { message = "Kabin ekibi silinirken hata oluştu" });
            }
        }
    }
}