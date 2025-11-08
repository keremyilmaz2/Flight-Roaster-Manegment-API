using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Models.Enums;

namespace FlightRosterAPI.Repositories.Interfaces
{
    public interface IPilotRepository : IRepository<Pilot>
    {
        Task<Pilot?> GetPilotWithUserAsync(int pilotId);
        Task<Pilot?> GetByUserIdAsync(int userId);
        Task<Pilot?> GetByLicenseNumberAsync(string licenseNumber);
        Task<IEnumerable<Pilot>> GetActivePilotsAsync();
        Task<IEnumerable<Pilot>> GetPilotsBySeniorityAsync(PilotSeniority seniority);
        Task<IEnumerable<Pilot>> GetAvailablePilotsForFlightAsync(int aircraftId, double distance);
        Task<IEnumerable<Pilot>> GetPilotsWithExpiredLicensesAsync();
        Task<bool> IsLicenseNumberUniqueAsync(string licenseNumber, int? excludeId = null);
    }
}