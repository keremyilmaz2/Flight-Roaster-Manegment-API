using FlightRosterAPI.Models.DTOs.Aircraft;

namespace FlightRosterAPI.Services.IServices
{
    public interface IAircraftService
    {
        Task<AircraftResponseDto?> GetAircraftByIdAsync(int aircraftId);
        Task<AircraftResponseDto?> GetAircraftByRegistrationNumberAsync(string registrationNumber);
        Task<IEnumerable<AircraftResponseDto>> GetAllAircraftsAsync();
        Task<IEnumerable<AircraftResponseDto>> GetActiveAircraftsAsync();
        Task<IEnumerable<AircraftResponseDto>> GetAircraftsByTypeAsync(string aircraftType);
        Task<AircraftResponseDto> CreateAircraftAsync(CreateAircraftDto createDto);
        Task<AircraftResponseDto> UpdateAircraftAsync(int aircraftId, UpdateAircraftDto updateDto);
        Task<bool> DeleteAircraftAsync(int aircraftId);
        Task<bool> ActivateAircraftAsync(int aircraftId);
        Task<bool> DeactivateAircraftAsync(int aircraftId);
    }
}