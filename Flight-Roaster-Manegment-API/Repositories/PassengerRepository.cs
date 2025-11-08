using FlightRosterAPI.Data;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightRosterAPI.Repositories
{
    public class PassengerRepository : Repository<Passenger>, IPassengerRepository
    {
        public PassengerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Passenger?> GetPassengerWithUserAsync(int passengerId)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PassengerId == passengerId);
        }

        public async Task<Passenger?> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<Passenger?> GetByPassportNumberAsync(string passportNumber)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PassportNumber == passportNumber);
        }

        public async Task<IEnumerable<Passenger>> GetActivePassengersAsync()
        {
            return await _dbSet
                .Include(p => p.User)
                .Where(p => p.IsActive && p.User.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Passenger>> GetPassengersByFlightAsync(int flightId)
        {
            return await _dbSet
                .Include(p => p.User)
                .Include(p => p.Seats)
                .Where(p => p.Seats.Any(s => s.FlightId == flightId))
                .ToListAsync();
        }

        public async Task<bool> IsPassportNumberUniqueAsync(string passportNumber, int? excludeId = null)
        {
            if (string.IsNullOrEmpty(passportNumber))
                return true;

            if (excludeId.HasValue)
            {
                return !await _dbSet.AnyAsync(p =>
                    p.PassportNumber == passportNumber &&
                    p.PassengerId != excludeId.Value);
            }

            return !await _dbSet.AnyAsync(p => p.PassportNumber == passportNumber);
        }
    }
}