using FlightRosterAPI.Models.Entities;

namespace FlightRosterAPI.Repositories.Interfaces
{
    public interface IFlightRepository : IRepository<Flight>
    {
        Task<Flight?> GetFlightWithDetailsAsync(int flightId);
        Task<Flight?> GetByFlightNumberAsync(string flightNumber);
        Task<IEnumerable<Flight>> GetFlightsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Flight>> GetFlightsByAircraftAsync(int aircraftId);
        Task<IEnumerable<Flight>> GetActiveFlightsAsync();
        Task<IEnumerable<Flight>> SearchFlightsAsync(string? flightNumber, string? departure, string? arrival);
    }
}