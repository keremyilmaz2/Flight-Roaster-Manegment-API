using FlightRosterAPI.Data;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightRosterAPI.Repositories
{
    public class FlightRepository : Repository<Flight>, IFlightRepository
    {
        public FlightRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Flight?> GetFlightWithDetailsAsync(int flightId)
        {
            return await _dbSet
                .Include(f => f.Aircraft)
                .Include(f => f.Seats)
                    .ThenInclude(s => s.Passenger)
                        .ThenInclude(p => p!.User)
                .Include(f => f.FlightCrews)
                    .ThenInclude(fc => fc.Pilot)
                        .ThenInclude(p => p.User)
                .Include(f => f.FlightCabinCrews)
                    .ThenInclude(fcc => fcc.CabinCrew)
                        .ThenInclude(cc => cc.User)
                .FirstOrDefaultAsync(f => f.FlightId == flightId);
        }

        public async Task<Flight?> GetByFlightNumberAsync(string flightNumber)
        {
            return await _dbSet
                .Include(f => f.Aircraft)
                .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);
        }

        public async Task<IEnumerable<Flight>> GetFlightsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(f => f.Aircraft)
                .Where(f => f.DepartureTime >= startDate && f.DepartureTime <= endDate)
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Flight>> GetFlightsByAircraftAsync(int aircraftId)
        {
            return await _dbSet
                .Where(f => f.AircraftId == aircraftId)
                .OrderByDescending(f => f.DepartureTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Flight>> GetActiveFlightsAsync()
        {
            return await _dbSet
                .Include(f => f.Aircraft)
                .Where(f => f.IsActive && f.DepartureTime >= DateTime.UtcNow)
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Flight>> SearchFlightsAsync(string? flightNumber, string? departure, string? arrival)
        {
            var query = _dbSet.Include(f => f.Aircraft).Where(f => f.IsActive);

            if (!string.IsNullOrEmpty(flightNumber))
            {
                query = query.Where(f => f.FlightNumber.Contains(flightNumber));
            }

            if (!string.IsNullOrEmpty(departure))
            {
                query = query.Where(f =>
                    f.DepartureCity.Contains(departure) ||
                    f.DepartureAirportCode.Contains(departure));
            }

            if (!string.IsNullOrEmpty(arrival))
            {
                query = query.Where(f =>
                    f.ArrivalCity.Contains(arrival) ||
                    f.ArrivalAirportCode.Contains(arrival));
            }

            return await query
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();
        }
    }
}