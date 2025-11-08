using FlightRosterAPI.Data;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Models.Enums;
using FlightRosterAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightRosterAPI.Repositories
{
    public class PilotRepository : Repository<Pilot>, IPilotRepository
    {
        public PilotRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Pilot?> GetPilotWithUserAsync(int pilotId)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PilotId == pilotId);
        }

        public async Task<Pilot?> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<Pilot?> GetByLicenseNumberAsync(string licenseNumber)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.LicenseNumber == licenseNumber);
        }

        public async Task<IEnumerable<Pilot>> GetActivePilotsAsync()
        {
            return await _dbSet
                .Include(p => p.User)
                .Where(p => p.IsActive && p.User.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pilot>> GetPilotsBySeniorityAsync(PilotSeniority seniority)
        {
            return await _dbSet
                .Include(p => p.User)
                .Where(p => p.Seniority == seniority && p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pilot>> GetAvailablePilotsForFlightAsync(int aircraftId, double distance)
        {
            var aircraft = await _context.Aircrafts.FindAsync(aircraftId);
            if (aircraft == null) return new List<Pilot>();

            return await _dbSet
                .Include(p => p.User)
                .Where(p => p.IsActive &&
                           p.User.IsActive &&
                           p.MaxFlightDistanceKm >= distance &&
                           p.QualifiedAircraftTypes.Contains(aircraft.AircraftType) &&
                           p.LicenseExpiryDate > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pilot>> GetPilotsWithExpiredLicensesAsync()
        {
            return await _dbSet
                .Include(p => p.User)
                .Where(p => p.LicenseExpiryDate <= DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<bool> IsLicenseNumberUniqueAsync(string licenseNumber, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return !await _dbSet.AnyAsync(p =>
                    p.LicenseNumber == licenseNumber &&
                    p.PilotId != excludeId.Value);
            }

            return !await _dbSet.AnyAsync(p => p.LicenseNumber == licenseNumber);
        }
    }
}