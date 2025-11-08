using FlightRosterAPI.Models.DTOs.Pilot;
using FlightRosterAPI.Models.Enums;

namespace FlightRosterAPI.Services.IServices
{
    public interface IPilotService
    {
        Task<PilotResponseDto?> GetPilotByIdAsync(int pilotId);
        Task<PilotResponseDto?> GetPilotByUserIdAsync(int userId);
        Task<PilotResponseDto?> GetPilotByLicenseNumberAsync(string licenseNumber);
        Task<IEnumerable<PilotResponseDto>> GetAllPilotsAsync();
        Task<IEnumerable<PilotResponseDto>> GetActivePilotsAsync();
        Task<IEnumerable<PilotResponseDto>> GetPilotsBySeniorityAsync(PilotSeniority seniority);
        Task<IEnumerable<PilotResponseDto>> GetAvailablePilotsForFlightAsync(int flightId);
        Task<IEnumerable<PilotResponseDto>> GetPilotsWithExpiredLicensesAsync();
        Task<PilotResponseDto> CreatePilotAsync(CreatePilotDto createDto);
        Task<PilotResponseDto> UpdatePilotAsync(int pilotId, UpdatePilotDto updateDto);
        Task<bool> DeletePilotAsync(int pilotId);
    }

    
}