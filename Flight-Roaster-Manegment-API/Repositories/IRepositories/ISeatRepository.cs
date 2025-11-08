using Flight_Roaster_Manegment_API.Models.Enums;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Models.Enums;

namespace FlightRosterAPI.Repositories.Interfaces
{
    public interface ISeatRepository : IRepository<Seat>
    {
        Task<Seat?> GetSeatWithDetailsAsync(int seatId);
        Task<IEnumerable<Seat>> GetSeatsByFlightAsync(int flightId);
        Task<IEnumerable<Seat>> GetAvailableSeatsByFlightAsync(int flightId);
        Task<IEnumerable<Seat>> GetOccupiedSeatsByFlightAsync(int flightId);
        Task<Seat?> GetSeatByFlightAndSeatNumberAsync(int flightId, string seatNumber);
        Task<IEnumerable<Seat>> GetSeatsByClassAsync(int flightId, SeatClass seatClass);
        Task<int> GetAvailableSeatsCountAsync(int flightId, SeatClass? seatClass = null);
        Task<bool> IsSeatNumberUniqueAsync(int flightId, string seatNumber, int? excludeId = null);
    }
}