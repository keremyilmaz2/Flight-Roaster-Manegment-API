using FlightRosterAPI.Data;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightRosterAPI.Repositories
{
    public class AircraftRepository : Repository<Aircraft>, IAircraftRepository
    {
        public AircraftRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Aircraft?> GetByRegistrationNumberAsync(string registrationNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.RegistrationNumber == registrationNumber);
        }

        public async Task<IEnumerable<Aircraft>> GetActiveAircraftsAsync()
        {
            return await _dbSet
                .Where(a => a.IsActive)
                .OrderBy(a => a.AircraftType)
                .ToListAsync();
        }

        public async Task<IEnumerable<Aircraft>> GetByAircraftTypeAsync(string aircraftType)
        {
            return await _dbSet
                .Where(a => a.AircraftType.Contains(aircraftType) && a.IsActive)
                .ToListAsync();
        }

        public async Task<bool> IsRegistrationNumberUniqueAsync(string registrationNumber, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return !await _dbSet.AnyAsync(a =>
                    a.RegistrationNumber == registrationNumber &&
                    a.AircraftId != excludeId.Value);
            }

            return !await _dbSet.AnyAsync(a => a.RegistrationNumber == registrationNumber);
        }
    }
}