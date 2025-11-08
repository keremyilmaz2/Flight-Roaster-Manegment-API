using FlightRosterAPI.Data;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Models.Enums;
using FlightRosterAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightRosterAPI.Repositories
{
    public class SeatRepository : Repository<Seat>, ISeatRepository
    {
        public SeatRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Seat?> GetSeatWithDetailsAsync(int seatId)
        {
            return await _dbSet
                .Include(s => s.Flight)
                    .ThenInclude(f => f.Aircraft)
                .Include(s => s.Passenger)
                    .ThenInclude(p => p!.User)
                .Include(s => s.ParentPassenger)
                    .ThenInclude(p => p!.User)
                .FirstOrDefaultAsync(s => s.SeatId == seatId);
        }

        public async Task<IEnumerable<Seat>> GetSeatsByFlightAsync(int flightId)
        {
            return await _dbSet
                .Include(s => s.Passenger)
                    .ThenInclude(p => p!.User)
                .Include(s => s.ParentPassenger)
                    .ThenInclude(p => p!.User)
                .Where(s => s.FlightId == flightId)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Seat>> GetAvailableSeatsByFlightAsync(int flightId)
        {
            return await _dbSet
                .Where(s => s.FlightId == flightId && !s.IsOccupied)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Seat>> GetOccupiedSeatsByFlightAsync(int flightId)
        {
            return await _dbSet
                .Include(s => s.Passenger)
                    .ThenInclude(p => p!.User)
                .Where(s => s.FlightId == flightId && s.IsOccupied)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();
        }

        public async Task<Seat?> GetSeatByFlightAndSeatNumberAsync(int flightId, string seatNumber)
        {
            return await _dbSet
                .Include(s => s.Passenger)
                    .ThenInclude(p => p!.User)
                .FirstOrDefaultAsync(s => s.FlightId == flightId && s.SeatNumber == seatNumber);
        }

        public async Task<IEnumerable<Seat>> GetSeatsByClassAsync(int flightId, SeatClass seatClass)
        {
            return await _dbSet
                .Where(s => s.FlightId == flightId && s.SeatClass == seatClass)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();
        }

        public async Task<int> GetAvailableSeatsCountAsync(int flightId, SeatClass? seatClass = null)
        {
            var query = _dbSet.Where(s => s.FlightId == flightId && !s.IsOccupied);

            if (seatClass.HasValue)
            {
                query = query.Where(s => s.SeatClass == seatClass.Value);
            }

            return await query.CountAsync();
        }

        public async Task<bool> IsSeatNumberUniqueAsync(int flightId, string seatNumber, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return !await _dbSet.AnyAsync(s =>
                    s.FlightId == flightId &&
                    s.SeatNumber == seatNumber &&
                    s.SeatId != excludeId.Value);
            }

            return !await _dbSet.AnyAsync(s => s.FlightId == flightId && s.SeatNumber == seatNumber);
        }
    }
}