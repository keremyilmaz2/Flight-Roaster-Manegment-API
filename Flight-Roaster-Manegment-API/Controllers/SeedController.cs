using FlightRosterAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightRosterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly IDatabaseSeeder _seeder;
        private readonly ILogger<SeedController> _logger;

        public SeedController(IDatabaseSeeder seeder, ILogger<SeedController> logger)
        {
            _seeder = seeder;
            _logger = logger;
        }

        /// <summary>
        /// Veritabanının dolu olup olmadığını kontrol eder
        /// </summary>
        [HttpGet("status")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSeedStatus()
        {
            try
            {
                var hasData = await _seeder.HasDataAsync();
                return Ok(new
                {
                    hasData,
                    message = hasData ? "Veritabanı zaten test verileri içeriyor." : "Veritabanı boş."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking seed status");
                return StatusCode(500, new { message = "Seed durumu kontrol edilirken hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Test verilerini veritabanına ekler (sadece veritabanı boşsa)
        /// </summary>
        [HttpPost("seed")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SeedTestData()
        {
            try
            {
                if (await _seeder.HasDataAsync())
                {
                    return BadRequest(new { message = "Veritabanı zaten test verileri içeriyor." });
                }

                _logger.LogInformation("Starting database seeding...");
                await _seeder.SeedTestDataAsync();

                return Ok(new
                {
                    message = "Test verileri başarıyla eklendi!",
                    details = new
                    {
                        aircrafts = "15 uçak",
                        pilots = "20 pilot",
                        cabinCrew = "50 kabin görevlisi",
                        passengers = "100 yolcu",
                        flights = "30 uçuş",
                        admin = "admin@flightroster.com / Admin123!"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding test data");
                return StatusCode(500, new { message = "Test verileri eklenirken hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Test verilerini zorla ekler (veritabanı doluysa bile)
        /// </summary>
        [HttpPost("seed-force")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForceSeedTestData()
        {
            try
            {
                _logger.LogWarning("Force seeding test data - bu işlem duplicate veriler oluşturabilir!");
                await _seeder.SeedTestDataAsync();

                return Ok(new
                {
                    message = "Test verileri zorla eklendi!",
                    warning = "Bu işlem duplicate veriler oluşturmuş olabilir."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error force seeding test data");
                return StatusCode(500, new { message = "Test verileri zorla eklenirken hata oluştu.", error = ex.Message });
            }
        }
    }
}