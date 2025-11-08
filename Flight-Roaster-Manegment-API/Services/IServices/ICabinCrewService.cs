using FlightRosterAPI.Models.DTOs.CabinCrew;
using FlightRosterAPI.Models.Enums;

namespace FlightRosterAPI.Services.IServices
{
    public interface ICabinCrewService
    {
        Task<CabinCrewResponseDto?> GetCabinCrewByIdAsync(int cabinCrewId);
        Task<CabinCrewResponseDto?> GetCabinCrewByUserIdAsync(int userId);
        Task<IEnumerable<CabinCrewResponseDto>> GetAllCabinCrewAsync();
        Task<IEnumerable<CabinCrewResponseDto>> GetActiveCabinCrewAsync();
        Task<IEnumerable<CabinCrewResponseDto>> GetCabinCrewByTypeAsync(CabinCrewType crewType);
        Task<IEnumerable<CabinCrewResponseDto>> GetCabinCrewBySeniorityAsync(CabinCrewSeniority seniority);
        Task<IEnumerable<CabinCrewResponseDto>> GetAvailableCabinCrewForFlightAsync(int flightId);
        Task<IEnumerable<CabinCrewResponseDto>> GetChefsWithRecipesAsync();
        Task<CabinCrewResponseDto> CreateCabinCrewAsync(CreateCabinCrewDto createDto);
        Task<CabinCrewResponseDto> UpdateCabinCrewAsync(int cabinCrewId, UpdateCabinCrewDto updateDto);
        Task<bool> DeleteCabinCrewAsync(int cabinCrewId);
    }
}