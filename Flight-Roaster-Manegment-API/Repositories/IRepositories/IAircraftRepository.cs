using FlightRosterAPI.Models.Entities;

namespace FlightRosterAPI.Repositories.Interfaces
{
    public interface IAircraftRepository : IRepository<Aircraft>
    {
        Task<Aircraft?> GetByRegistrationNumberAsync(string registrationNumber);
        Task<IEnumerable<Aircraft>> GetActiveAircraftsAsync();
        Task<IEnumerable<Aircraft>> GetByAircraftTypeAsync(string aircraftType);
        Task<bool> IsRegistrationNumberUniqueAsync(string registrationNumber, int? excludeId = null);
    }
}