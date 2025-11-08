using FlightRosterAPI.Models.Entities;

namespace FlightRosterAPI.Repositories.Interfaces
{
    public interface IPassengerRepository : IRepository<Passenger>
    {
        Task<Passenger?> GetPassengerWithUserAsync(int passengerId);
        Task<Passenger?> GetByUserIdAsync(int userId);
        Task<Passenger?> GetByPassportNumberAsync(string passportNumber);
        Task<IEnumerable<Passenger>> GetActivePassengersAsync();
        Task<IEnumerable<Passenger>> GetPassengersByFlightAsync(int flightId);
        Task<bool> IsPassportNumberUniqueAsync(string passportNumber, int? excludeId = null);
    }
}