using FlightRosterAPI.Models.DTOs.Pilot;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Models.Enums;
using FlightRosterAPI.Repositories.Interfaces;
using FlightRosterAPI.Services.IServices;

namespace FlightRosterAPI.Services
{
    public class PilotService : IPilotService
    {
        private readonly IPilotRepository _pilotRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly ILogger<PilotService> _logger;

        public PilotService(
            IPilotRepository pilotRepository,
            IFlightRepository flightRepository,
            ILogger<PilotService> logger)
        {
            _pilotRepository = pilotRepository;
            _flightRepository = flightRepository;
            _logger = logger;
        }

        public async Task<PilotResponseDto?> GetPilotByIdAsync(int pilotId)
        {
            var pilot = await _pilotRepository.GetPilotWithUserAsync(pilotId);
            return pilot != null ? MapToResponseDto(pilot) : null;
        }

        public async Task<PilotResponseDto?> GetPilotByUserIdAsync(int userId)
        {
            var pilot = await _pilotRepository.GetByUserIdAsync(userId);
            return pilot != null ? MapToResponseDto(pilot) : null;
        }

        public async Task<PilotResponseDto?> GetPilotByLicenseNumberAsync(string licenseNumber)
        {
            var pilot = await _pilotRepository.GetByLicenseNumberAsync(licenseNumber);
            return pilot != null ? MapToResponseDto(pilot) : null;
        }

        public async Task<IEnumerable<PilotResponseDto>> GetAllPilotsAsync()
        {
            var pilots = await _pilotRepository.GetAllAsync();
            var result = new List<PilotResponseDto>();

            foreach (var pilot in pilots)
            {
                var pilotWithUser = await _pilotRepository.GetPilotWithUserAsync(pilot.PilotId);
                if (pilotWithUser != null)
                    result.Add(MapToResponseDto(pilotWithUser));
            }

            return result;
        }

        public async Task<IEnumerable<PilotResponseDto>> GetActivePilotsAsync()
        {
            var pilots = await _pilotRepository.GetActivePilotsAsync();
            return pilots.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<PilotResponseDto>> GetPilotsBySeniorityAsync(PilotSeniority seniority)
        {
            var pilots = await _pilotRepository.GetPilotsBySeniorityAsync(seniority);
            return pilots.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<PilotResponseDto>> GetAvailablePilotsForFlightAsync(int flightId)
        {
            var flight = await _flightRepository.GetFlightWithDetailsAsync(flightId);
            if (flight == null)
                throw new KeyNotFoundException("Uçuş bulunamadı");

            var pilots = await _pilotRepository.GetAvailablePilotsForFlightAsync(
                flight.AircraftId,
                flight.DistanceKm);

            return pilots.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<PilotResponseDto>> GetPilotsWithExpiredLicensesAsync()
        {
            var pilots = await _pilotRepository.GetPilotsWithExpiredLicensesAsync();
            return pilots.Select(MapToResponseDto);
        }

        public async Task<PilotResponseDto> CreatePilotAsync(CreatePilotDto createDto)
        {
            // Validate license number uniqueness
            var isUnique = await _pilotRepository.IsLicenseNumberUniqueAsync(createDto.LicenseNumber);
            if (!isUnique)
                throw new InvalidOperationException("Bu lisans numarası zaten kullanılıyor");

            // Validate license expiry date
            if (createDto.LicenseExpiryDate <= DateTime.UtcNow)
                throw new InvalidOperationException("Lisans son kullanma tarihi gelecekte olmalı");

            // Validate qualified aircraft types
            if (string.IsNullOrWhiteSpace(createDto.QualifiedAircraftTypes))
                throw new InvalidOperationException("En az bir uçak tipi belirtilmeli");

            var pilot = new Pilot
            {
                UserId = createDto.UserId,
                LicenseNumber = createDto.LicenseNumber,
                Seniority = createDto.Seniority,
                MaxFlightDistanceKm = createDto.MaxFlightDistanceKm,
                QualifiedAircraftTypes = createDto.QualifiedAircraftTypes,
                TotalFlightHours = createDto.TotalFlightHours,
                LicenseExpiryDate = createDto.LicenseExpiryDate,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _pilotRepository.AddAsync(pilot);
            _logger.LogInformation("Pilot created: {LicenseNumber}", pilot.LicenseNumber);

            var createdPilot = await _pilotRepository.GetPilotWithUserAsync(pilot.PilotId);
            return MapToResponseDto(createdPilot!);
        }

        public async Task<PilotResponseDto> UpdatePilotAsync(int pilotId, UpdatePilotDto updateDto)
        {
            var pilot = await _pilotRepository.GetPilotWithUserAsync(pilotId);
            if (pilot == null)
                throw new KeyNotFoundException("Pilot bulunamadı");

            // Check license number uniqueness if being updated
            if (!string.IsNullOrEmpty(updateDto.LicenseNumber) &&
                updateDto.LicenseNumber != pilot.LicenseNumber)
            {
                var isUnique = await _pilotRepository.IsLicenseNumberUniqueAsync(
                    updateDto.LicenseNumber, pilotId);
                if (!isUnique)
                    throw new InvalidOperationException("Bu lisans numarası zaten kullanılıyor");

                pilot.LicenseNumber = updateDto.LicenseNumber;
            }

            if (updateDto.Seniority.HasValue)
                pilot.Seniority = updateDto.Seniority.Value;

            if (updateDto.MaxFlightDistanceKm.HasValue)
                pilot.MaxFlightDistanceKm = updateDto.MaxFlightDistanceKm.Value;

            if (!string.IsNullOrEmpty(updateDto.QualifiedAircraftTypes))
                pilot.QualifiedAircraftTypes = updateDto.QualifiedAircraftTypes;

            if (updateDto.TotalFlightHours.HasValue)
                pilot.TotalFlightHours = updateDto.TotalFlightHours.Value;

            if (updateDto.LicenseExpiryDate.HasValue)
            {
                if (updateDto.LicenseExpiryDate.Value <= DateTime.UtcNow)
                    throw new InvalidOperationException("Lisans son kullanma tarihi gelecekte olmalı");

                pilot.LicenseExpiryDate = updateDto.LicenseExpiryDate.Value;
            }

            if (updateDto.IsActive.HasValue)
                pilot.IsActive = updateDto.IsActive.Value;

            pilot.UpdatedAt = DateTime.UtcNow;

            await _pilotRepository.UpdateAsync(pilot);
            _logger.LogInformation("Pilot updated: {PilotId}", pilotId);

            var updatedPilot = await _pilotRepository.GetPilotWithUserAsync(pilotId);
            return MapToResponseDto(updatedPilot!);
        }

        public async Task<bool> DeletePilotAsync(int pilotId)
        {
            var pilot = await _pilotRepository.GetByIdAsync(pilotId);
            if (pilot == null)
                throw new KeyNotFoundException("Pilot bulunamadı");

            await _pilotRepository.DeleteAsync(pilot);
            _logger.LogInformation("Pilot deleted: {PilotId}", pilotId);

            return true;
        }

        private PilotResponseDto MapToResponseDto(Pilot pilot)
        {
            var qualifiedTypes = pilot.QualifiedAircraftTypes
                .Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();

            return new PilotResponseDto
            {
                PilotId = pilot.PilotId,
                UserId = pilot.UserId,
                FirstName = pilot.User.FirstName,
                LastName = pilot.User.LastName,
                Email = pilot.User.Email!,
                LicenseNumber = pilot.LicenseNumber,
                Seniority = pilot.Seniority,
                MaxFlightDistanceKm = pilot.MaxFlightDistanceKm,
                QualifiedAircraftTypes = qualifiedTypes,
                TotalFlightHours = pilot.TotalFlightHours,
                LicenseExpiryDate = pilot.LicenseExpiryDate,
                CreatedAt = pilot.CreatedAt,
                IsActive = pilot.IsActive
            };
        }
    }
}