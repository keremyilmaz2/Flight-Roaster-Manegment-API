using FlightRosterAPI.Models;
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

        [HttpGet]
        public async Task<IActionResult> GetAllAircrafts()
        {
            var response = new ResponseDto();
            try
            {
                var aircrafts = await _aircraftService.GetAllAircraftsAsync();
                response.Result = aircrafts;
                response.Message = "Uçaklar başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all aircrafts");
                response.IsSuccess = false;
                response.Message = "Uçaklar getirilirken bir hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveAircrafts()
        {
            var response = new ResponseDto();
            try
            {
                var aircrafts = await _aircraftService.GetActiveAircraftsAsync();
                response.Result = aircrafts;
                response.Message = "Aktif uçaklar başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active aircrafts");
                response.IsSuccess = false;
                response.Message = "Aktif uçaklar getirilirken bir hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAircraftById(int id)
        {
            var response = new ResponseDto();
            try
            {
                var aircraft = await _aircraftService.GetAircraftByIdAsync(id);

                if (aircraft == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"ID {id} ile uçak bulunamadı";
                    return NotFound(response);
                }

                response.Result = aircraft;
                response.Message = "Uçak başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving aircraft {AircraftId}", id);
                response.IsSuccess = false;
                response.Message = "Uçak getirilirken bir hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("registration/{registrationNumber}")]
        public async Task<IActionResult> GetAircraftByRegistrationNumber(string registrationNumber)
        {
            var response = new ResponseDto();
            try
            {
                var aircraft = await _aircraftService.GetAircraftByRegistrationNumberAsync(registrationNumber);

                if (aircraft == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"Kayıt numarası {registrationNumber} ile uçak bulunamadı";
                    return NotFound(response);
                }

                response.Result = aircraft;
                response.Message = "Uçak başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving aircraft by registration {RegistrationNumber}", registrationNumber);
                response.IsSuccess = false;
                response.Message = "Uçak getirilirken bir hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpGet("type/{aircraftType}")]
        public async Task<IActionResult> GetAircraftsByType(string aircraftType)
        {
            var response = new ResponseDto();
            try
            {
                var aircrafts = await _aircraftService.GetAircraftsByTypeAsync(aircraftType);
                response.Result = aircrafts;
                response.Message = "Uçaklar başarıyla getirildi";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving aircrafts by type {AircraftType}", aircraftType);
                response.IsSuccess = false;
                response.Message = "Uçaklar getirilirken bir hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateAircraft([FromBody] CreateAircraftDto createDto)
        {
            var response = new ResponseDto();
            try
            {
                var aircraft = await _aircraftService.CreateAircraftAsync(createDto);
                response.Result = aircraft;
                response.Message = "Uçak başarıyla oluşturuldu";
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
                _logger.LogError(ex, "Error creating aircraft");
                response.IsSuccess = false;
                response.Message = "Uçak oluşturulurken bir hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateAircraft(int id, [FromBody] UpdateAircraftDto updateDto)
        {
            var response = new ResponseDto();
            try
            {
                var aircraft = await _aircraftService.UpdateAircraftAsync(id, updateDto);
                response.Result = aircraft;
                response.Message = "Uçak başarıyla güncellendi";
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
                _logger.LogError(ex, "Error updating aircraft {AircraftId}", id);
                response.IsSuccess = false;
                response.Message = "Uçak güncellenirken bir hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteAircraft(int id)
        {
            var response = new ResponseDto();
            try
            {
                await _aircraftService.DeleteAircraftAsync(id);
                response.Message = "Uçak başarıyla silindi";
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
                _logger.LogError(ex, "Error deleting aircraft {AircraftId}", id);
                response.IsSuccess = false;
                response.Message = "Uçak silinirken bir hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPatch("{id}/activate")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ActivateAircraft(int id)
        {
            var response = new ResponseDto();
            try
            {
                await _aircraftService.ActivateAircraftAsync(id);
                response.Message = "Uçak başarıyla aktifleştirildi";
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
                _logger.LogError(ex, "Error activating aircraft {AircraftId}", id);
                response.IsSuccess = false;
                response.Message = "Uçak aktifleştirilirken bir hata oluştu";
                return StatusCode(500, response);
            }
        }

        [HttpPatch("{id}/deactivate")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeactivateAircraft(int id)
        {
            var response = new ResponseDto();
            try
            {
                await _aircraftService.DeactivateAircraftAsync(id);
                response.Message = "Uçak başarıyla deaktif edildi";
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
                _logger.LogError(ex, "Error deactivating aircraft {AircraftId}", id);
                response.IsSuccess = false;
                response.Message = "Uçak deaktif edilirken bir hata oluştu";
                return StatusCode(500, response);
            }
        }
    }
}