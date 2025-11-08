using FlightRosterAPI.Models.Entities;

namespace FlightRosterAPI.Repositories.Interfaces
{
    public interface IFlightCrewRepository : IRepository<FlightCrew>
    {
        Task<IEnumerable<FlightCrew>> GetCrewByFlightAsync(int flightId);
        Task<IEnumerable<FlightCrew>> GetFlightsByPilotAsync(int pilotId);
        Task<FlightCrew?> GetFlightCrewWithDetailsAsync(int flightCrewId);
        Task<bool> IsPilotAssignedToFlightAsync(int flightId, int pilotId);
        Task RemoveCrewFromFlightAsync(int flightId, int pilotId);
    }
}