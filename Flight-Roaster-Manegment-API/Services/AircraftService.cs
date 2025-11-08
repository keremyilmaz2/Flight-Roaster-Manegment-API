using FlightRosterAPI.Models.DTOs.Aircraft;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Repositories.Interfaces;
using FlightRosterAPI.Services.IServices;

namespace FlightRosterAPI.Services
{
    public class AircraftService : IAircraftService
    {
        private readonly IAircraftRepository _aircraftRepository;
        private readonly ILogger<AircraftService> _logger;

        public AircraftService(
            IAircraftRepository aircraftRepository,
            ILogger<AircraftService> logger)
        {
            _aircraftRepository = aircraftRepository;
            _logger = logger;
        }

        public async Task<AircraftResponseDto?> GetAircraftByIdAsync(int aircraftId)
        {
            var aircraft = await _aircraftRepository.GetByIdAsync(aircraftId);
            return aircraft != null ? MapToResponseDto(aircraft) : null;
        }

        public async Task<AircraftResponseDto?> GetAircraftByRegistrationNumberAsync(string registrationNumber)
        {
            var aircraft = await _aircraftRepository.GetByRegistrationNumberAsync(registrationNumber);
            return aircraft != null ? MapToResponseDto(aircraft) : null;
        }

        public async Task<IEnumerable<AircraftResponseDto>> GetAllAircraftsAsync()
        {
            var aircrafts = await _aircraftRepository.GetAllAsync();
            return aircrafts.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<AircraftResponseDto>> GetActiveAircraftsAsync()
        {
            var aircrafts = await _aircraftRepository.GetActiveAircraftsAsync();
            return aircrafts.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<AircraftResponseDto>> GetAircraftsByTypeAsync(string aircraftType)
        {
            var aircrafts = await _aircraftRepository.GetByAircraftTypeAsync(aircraftType);
            return aircrafts.Select(MapToResponseDto);
        }

        public async Task<AircraftResponseDto> CreateAircraftAsync(CreateAircraftDto createDto)
        {
            // Validate registration number uniqueness
            var isUnique = await _aircraftRepository.IsRegistrationNumberUniqueAsync(createDto.RegistrationNumber);
            if (!isUnique)
                throw new InvalidOperationException("Bu kayıt numarası zaten kullanılıyor");

            // Validate seat counts
            if (createDto.BusinessClassSeats + createDto.EconomyClassSeats != createDto.TotalSeats)
                throw new InvalidOperationException("Business ve Economy koltuk sayıları toplamı, toplam koltuk sayısına eşit olmalı");

            // Validate crew requirements
            if (createDto.MinCrewRequired > createDto.MaxCrewCapacity)
                throw new InvalidOperationException("Minimum mürettebat sayısı, maksimum kapasiteden büyük olamaz");

            if (createDto.MinCabinCrewRequired > createDto.MaxCabinCrewCapacity)
                throw new InvalidOperationException("Minimum kabin ekibi sayısı, maksimum kapasiteden büyük olamaz");

            var aircraft = new Aircraft
            {
                AircraftType = createDto.AircraftType,
                RegistrationNumber = createDto.RegistrationNumber,
                TotalSeats = createDto.TotalSeats,
                BusinessClassSeats = createDto.BusinessClassSeats,
                EconomyClassSeats = createDto.EconomyClassSeats,
                MinCrewRequired = createDto.MinCrewRequired,
                MaxCrewCapacity = createDto.MaxCrewCapacity,
                MinCabinCrewRequired = createDto.MinCabinCrewRequired,
                MaxCabinCrewCapacity = createDto.MaxCabinCrewCapacity,
                MaxRangeKm = createDto.MaxRangeKm,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _aircraftRepository.AddAsync(aircraft);
            _logger.LogInformation("Aircraft created: {RegistrationNumber}", aircraft.RegistrationNumber);

            return MapToResponseDto(aircraft);
        }

        public async Task<AircraftResponseDto> UpdateAircraftAsync(int aircraftId, UpdateAircraftDto updateDto)
        {
            var aircraft = await _aircraftRepository.GetByIdAsync(aircraftId);
            if (aircraft == null)
                throw new KeyNotFoundException("Uçak bulunamadı");

            // Check registration number uniqueness if being updated
            if (!string.IsNullOrEmpty(updateDto.RegistrationNumber) &&
                updateDto.RegistrationNumber != aircraft.RegistrationNumber)
            {
                var isUnique = await _aircraftRepository.IsRegistrationNumberUniqueAsync(
                    updateDto.RegistrationNumber, aircraftId);
                if (!isUnique)
                    throw new InvalidOperationException("Bu kayıt numarası zaten kullanılıyor");

                aircraft.RegistrationNumber = updateDto.RegistrationNumber;
            }

            if (!string.IsNullOrEmpty(updateDto.AircraftType))
                aircraft.AircraftType = updateDto.AircraftType;

            if (updateDto.TotalSeats.HasValue)
                aircraft.TotalSeats = updateDto.TotalSeats.Value;

            if (updateDto.BusinessClassSeats.HasValue)
                aircraft.BusinessClassSeats = updateDto.BusinessClassSeats.Value;

            if (updateDto.EconomyClassSeats.HasValue)
                aircraft.EconomyClassSeats = updateDto.EconomyClassSeats.Value;

            // Validate seat counts after update
            if (aircraft.BusinessClassSeats + aircraft.EconomyClassSeats != aircraft.TotalSeats)
                throw new InvalidOperationException("Business ve Economy koltuk sayıları toplamı, toplam koltuk sayısına eşit olmalı");

            if (updateDto.MinCrewRequired.HasValue)
                aircraft.MinCrewRequired = updateDto.MinCrewRequired.Value;

            if (updateDto.MaxCrewCapacity.HasValue)
                aircraft.MaxCrewCapacity = updateDto.MaxCrewCapacity.Value;

            if (updateDto.MinCabinCrewRequired.HasValue)
                aircraft.MinCabinCrewRequired = updateDto.MinCabinCrewRequired.Value;

            if (updateDto.MaxCabinCrewCapacity.HasValue)
                aircraft.MaxCabinCrewCapacity = updateDto.MaxCabinCrewCapacity.Value;

            // Validate crew requirements after update
            if (aircraft.MinCrewRequired > aircraft.MaxCrewCapacity)
                throw new InvalidOperationException("Minimum mürettebat sayısı, maksimum kapasiteden büyük olamaz");

            if (aircraft.MinCabinCrewRequired > aircraft.MaxCabinCrewCapacity)
                throw new InvalidOperationException("Minimum kabin ekibi sayısı, maksimum kapasiteden büyük olamaz");

            if (updateDto.MaxRangeKm.HasValue)
                aircraft.MaxRangeKm = updateDto.MaxRangeKm.Value;

            if (updateDto.IsActive.HasValue)
                aircraft.IsActive = updateDto.IsActive.Value;

            aircraft.UpdatedAt = DateTime.UtcNow;

            await _aircraftRepository.UpdateAsync(aircraft);
            _logger.LogInformation("Aircraft updated: {AircraftId}", aircraftId);

            return MapToResponseDto(aircraft);
        }

        public async Task<bool> DeleteAircraftAsync(int aircraftId)
        {
            var aircraft = await _aircraftRepository.GetByIdAsync(aircraftId);
            if (aircraft == null)
                throw new KeyNotFoundException("Uçak bulunamadı");

            await _aircraftRepository.DeleteAsync(aircraft);
            _logger.LogInformation("Aircraft deleted: {AircraftId}", aircraftId);

            return true;
        }

        public async Task<bool> ActivateAircraftAsync(int aircraftId)
        {
            var aircraft = await _aircraftRepository.GetByIdAsync(aircraftId);
            if (aircraft == null)
                throw new KeyNotFoundException("Uçak bulunamadı");

            aircraft.IsActive = true;
            aircraft.UpdatedAt = DateTime.UtcNow;

            await _aircraftRepository.UpdateAsync(aircraft);
            _logger.LogInformation("Aircraft activated: {AircraftId}", aircraftId);

            return true;
        }

        public async Task<bool> DeactivateAircraftAsync(int aircraftId)
        {
            var aircraft = await _aircraftRepository.GetByIdAsync(aircraftId);
            if (aircraft == null)
                throw new KeyNotFoundException("Uçak bulunamadı");

            aircraft.IsActive = false;
            aircraft.UpdatedAt = DateTime.UtcNow;

            await _aircraftRepository.UpdateAsync(aircraft);
            _logger.LogInformation("Aircraft deactivated: {AircraftId}", aircraftId);

            return true;
        }

        private AircraftResponseDto MapToResponseDto(Aircraft aircraft)
        {
            return new AircraftResponseDto
            {
                AircraftId = aircraft.AircraftId,
                AircraftType = aircraft.AircraftType,
                RegistrationNumber = aircraft.RegistrationNumber,
                TotalSeats = aircraft.TotalSeats,
                BusinessClassSeats = aircraft.BusinessClassSeats,
                EconomyClassSeats = aircraft.EconomyClassSeats,
                MinCrewRequired = aircraft.MinCrewRequired,
                MaxCrewCapacity = aircraft.MaxCrewCapacity,
                MinCabinCrewRequired = aircraft.MinCabinCrewRequired,
                MaxCabinCrewCapacity = aircraft.MaxCabinCrewCapacity,
                MaxRangeKm = aircraft.MaxRangeKm,
                CreatedAt = aircraft.CreatedAt,
                IsActive = aircraft.IsActive
            };
        }
    }
}