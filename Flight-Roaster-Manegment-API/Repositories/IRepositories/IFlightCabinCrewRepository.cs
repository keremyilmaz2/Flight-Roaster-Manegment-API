using FlightRosterAPI.Models.Entities;

namespace FlightRosterAPI.Repositories.Interfaces
{
    public interface IFlightCabinCrewRepository : IRepository<FlightCabinCrew>
    {
        Task<IEnumerable<FlightCabinCrew>> GetCabinCrewByFlightAsync(int flightId);
        Task<IEnumerable<FlightCabinCrew>> GetFlightsByCabinCrewAsync(int cabinCrewId);
        Task<FlightCabinCrew?> GetFlightCabinCrewWithDetailsAsync(int flightCabinCrewId);
        Task<bool> IsCabinCrewAssignedToFlightAsync(int flightId, int cabinCrewId);
        Task RemoveCabinCrewFromFlightAsync(int flightId, int cabinCrewId);
    }
}