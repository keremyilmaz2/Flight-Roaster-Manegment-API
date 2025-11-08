using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Models.Enums;

namespace FlightRosterAPI.Repositories.Interfaces
{
    public interface ICabinCrewRepository : IRepository<CabinCrew>
    {
        Task<CabinCrew?> GetCabinCrewWithUserAsync(int cabinCrewId);
        Task<CabinCrew?> GetByUserIdAsync(int userId);
        Task<IEnumerable<CabinCrew>> GetActiveCabinCrewAsync();
        Task<IEnumerable<CabinCrew>> GetCabinCrewByTypeAsync(CabinCrewType crewType);
        Task<IEnumerable<CabinCrew>> GetCabinCrewBySeniorityAsync(CabinCrewSeniority seniority);
        Task<IEnumerable<CabinCrew>> GetAvailableCabinCrewForFlightAsync(string aircraftType);
        Task<IEnumerable<CabinCrew>> GetChefsWithRecipesAsync();
    }
}