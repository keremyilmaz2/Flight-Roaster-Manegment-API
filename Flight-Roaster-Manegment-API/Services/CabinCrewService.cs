using FlightRosterAPI.Models.DTOs.CabinCrew;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Models.Enums;
using FlightRosterAPI.Repositories.Interfaces;
using FlightRosterAPI.Services.IServices;

namespace FlightRosterAPI.Services
{
    public class CabinCrewService : ICabinCrewService
    {
        private readonly ICabinCrewRepository _cabinCrewRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly ILogger<CabinCrewService> _logger;

        public CabinCrewService(
            ICabinCrewRepository cabinCrewRepository,
            IFlightRepository flightRepository,
            ILogger<CabinCrewService> logger)
        {
            _cabinCrewRepository = cabinCrewRepository;
            _flightRepository = flightRepository;
            _logger = logger;
        }

        public async Task<CabinCrewResponseDto?> GetCabinCrewByIdAsync(int cabinCrewId)
        {
            var cabinCrew = await _cabinCrewRepository.GetCabinCrewWithUserAsync(cabinCrewId);
            return cabinCrew != null ? MapToResponseDto(cabinCrew) : null;
        }

        public async Task<CabinCrewResponseDto?> GetCabinCrewByUserIdAsync(int userId)
        {
            var cabinCrew = await _cabinCrewRepository.GetByUserIdAsync(userId);
            return cabinCrew != null ? MapToResponseDto(cabinCrew) : null;
        }

        public async Task<IEnumerable<CabinCrewResponseDto>> GetAllCabinCrewAsync()
        {
            var cabinCrews = await _cabinCrewRepository.GetAllAsync();
            var result = new List<CabinCrewResponseDto>();

            foreach (var crew in cabinCrews)
            {
                var crewWithUser = await _cabinCrewRepository.GetCabinCrewWithUserAsync(crew.CabinCrewId);
                if (crewWithUser != null)
                    result.Add(MapToResponseDto(crewWithUser));
            }

            return result;
        }

        public async Task<IEnumerable<CabinCrewResponseDto>> GetActiveCabinCrewAsync()
        {
            var cabinCrews = await _cabinCrewRepository.GetActiveCabinCrewAsync();
            return cabinCrews.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<CabinCrewResponseDto>> GetCabinCrewByTypeAsync(CabinCrewType crewType)
        {
            var cabinCrews = await _cabinCrewRepository.GetCabinCrewByTypeAsync(crewType);
            return cabinCrews.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<CabinCrewResponseDto>> GetCabinCrewBySeniorityAsync(CabinCrewSeniority seniority)
        {
            var cabinCrews = await _cabinCrewRepository.GetCabinCrewBySeniorityAsync(seniority);
            return cabinCrews.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<CabinCrewResponseDto>> GetAvailableCabinCrewForFlightAsync(int flightId)
        {
            var flight = await _flightRepository.GetFlightWithDetailsAsync(flightId);
            if (flight == null)
                throw new KeyNotFoundException("Uçuş bulunamadı");

            var cabinCrews = await _cabinCrewRepository.GetAvailableCabinCrewForFlightAsync(
                flight.Aircraft.AircraftType);

            return cabinCrews.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<CabinCrewResponseDto>> GetChefsWithRecipesAsync()
        {
            var chefs = await _cabinCrewRepository.GetChefsWithRecipesAsync();
            return chefs.Select(MapToResponseDto);
        }

        public async Task<CabinCrewResponseDto> CreateCabinCrewAsync(CreateCabinCrewDto createDto)
        {
            // Validate qualified aircraft types
            if (string.IsNullOrWhiteSpace(createDto.QualifiedAircraftTypes))
                throw new InvalidOperationException("En az bir uçak tipi belirtilmeli");

            // Validate chef has recipes
            if (createDto.CrewType == CabinCrewType.Chef)
            {
                if (string.IsNullOrWhiteSpace(createDto.Recipes))
                    throw new InvalidOperationException("Aşçı en az bir tarife sahip olmalı");

                var recipes = createDto.Recipes.Split(',').Select(r => r.Trim()).Where(r => !string.IsNullOrEmpty(r)).ToList();
                if (recipes.Count < 2 || recipes.Count > 4)
                    throw new InvalidOperationException("Aşçı 2-4 arasında tarife sahip olmalı");
            }

            var cabinCrew = new CabinCrew
            {
                UserId = createDto.UserId,
                CrewType = createDto.CrewType,
                Seniority = createDto.Seniority,
                QualifiedAircraftTypes = createDto.QualifiedAircraftTypes,
                Recipes = createDto.Recipes,
                Languages = createDto.Languages,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _cabinCrewRepository.AddAsync(cabinCrew);
            _logger.LogInformation("Cabin crew created: {CabinCrewId}", cabinCrew.CabinCrewId);

            var createdCrew = await _cabinCrewRepository.GetCabinCrewWithUserAsync(cabinCrew.CabinCrewId);
            return MapToResponseDto(createdCrew!);
        }

        public async Task<CabinCrewResponseDto> UpdateCabinCrewAsync(int cabinCrewId, UpdateCabinCrewDto updateDto)
        {
            var cabinCrew = await _cabinCrewRepository.GetCabinCrewWithUserAsync(cabinCrewId);
            if (cabinCrew == null)
                throw new KeyNotFoundException("Kabin ekibi bulunamadı");

            if (updateDto.CrewType.HasValue)
            {
                cabinCrew.CrewType = updateDto.CrewType.Value;

                // If changing to chef, validate recipes
                if (updateDto.CrewType.Value == CabinCrewType.Chef && string.IsNullOrWhiteSpace(cabinCrew.Recipes))
                    throw new InvalidOperationException("Aşçıya dönüştürülürken en az bir tarif belirtilmeli");
            }

            if (updateDto.Seniority.HasValue)
                cabinCrew.Seniority = updateDto.Seniority.Value;

            if (!string.IsNullOrEmpty(updateDto.QualifiedAircraftTypes))
                cabinCrew.QualifiedAircraftTypes = updateDto.QualifiedAircraftTypes;

            if (updateDto.Recipes != null)
            {
                if (cabinCrew.CrewType == CabinCrewType.Chef && !string.IsNullOrWhiteSpace(updateDto.Recipes))
                {
                    var recipes = updateDto.Recipes.Split(',').Select(r => r.Trim()).Where(r => !string.IsNullOrEmpty(r)).ToList();
                    if (recipes.Count < 2 || recipes.Count > 4)
                        throw new InvalidOperationException("Aşçı 2-4 arasında tarife sahip olmalı");
                }
                cabinCrew.Recipes = updateDto.Recipes;
            }

            if (updateDto.Languages != null)
                cabinCrew.Languages = updateDto.Languages;

            if (updateDto.IsActive.HasValue)
                cabinCrew.IsActive = updateDto.IsActive.Value;

            cabinCrew.UpdatedAt = DateTime.UtcNow;

            await _cabinCrewRepository.UpdateAsync(cabinCrew);
            _logger.LogInformation("Cabin crew updated: {CabinCrewId}", cabinCrewId);

            var updatedCrew = await _cabinCrewRepository.GetCabinCrewWithUserAsync(cabinCrewId);
            return MapToResponseDto(updatedCrew!);
        }

        public async Task<bool> DeleteCabinCrewAsync(int cabinCrewId)
        {
            var cabinCrew = await _cabinCrewRepository.GetByIdAsync(cabinCrewId);
            if (cabinCrew == null)
                throw new KeyNotFoundException("Kabin ekibi bulunamadı");

            await _cabinCrewRepository.DeleteAsync(cabinCrew);
            _logger.LogInformation("Cabin crew deleted: {CabinCrewId}", cabinCrewId);

            return true;
        }

        private CabinCrewResponseDto MapToResponseDto(CabinCrew cabinCrew)
        {
            var qualifiedTypes = cabinCrew.QualifiedAircraftTypes
                .Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();

            List<string>? recipes = null;
            if (!string.IsNullOrWhiteSpace(cabinCrew.Recipes))
            {
                recipes = cabinCrew.Recipes
                    .Split(',')
                    .Select(r => r.Trim())
                    .Where(r => !string.IsNullOrEmpty(r))
                    .ToList();
            }

            List<string>? languages = null;
            if (!string.IsNullOrWhiteSpace(cabinCrew.Languages))
            {
                languages = cabinCrew.Languages
                    .Split(',')
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrEmpty(l))
                    .ToList();
            }

            return new CabinCrewResponseDto
            {
                CabinCrewId = cabinCrew.CabinCrewId,
                UserId = cabinCrew.UserId,
                FirstName = cabinCrew.User.FirstName,
                LastName = cabinCrew.User.LastName,
                Email = cabinCrew.User.Email!,
                CrewType = cabinCrew.CrewType,
                Seniority = cabinCrew.Seniority,
                QualifiedAircraftTypes = qualifiedTypes,
                Recipes = recipes,
                Languages = languages,
                CreatedAt = cabinCrew.CreatedAt,
                IsActive = cabinCrew.IsActive
            };
        }
    }
}