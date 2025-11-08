using FlightRosterAPI.Data;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightRosterAPI.Repositories
{
    public class FlightCrewRepository : Repository<FlightCrew>, IFlightCrewRepository
    {
        public FlightCrewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<FlightCrew>> GetCrewByFlightAsync(int flightId)
        {
            return await _dbSet
                .Include(fc => fc.Pilot)
                    .ThenInclude(p => p.User)
                .Include(fc => fc.Flight)
                .Where(fc => fc.FlightId == flightId && fc.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<FlightCrew>> GetFlightsByPilotAsync(int pilotId)
        {
            return await _dbSet
                .Include(fc => fc.Flight)
                    .ThenInclude(f => f.Aircraft)
                .Include(fc => fc.Pilot)
                    .ThenInclude(p => p.User)
                .Where(fc => fc.PilotId == pilotId && fc.IsActive)
                .OrderByDescending(fc => fc.Flight.DepartureTime)
                .ToListAsync();
        }

        public async Task<FlightCrew?> GetFlightCrewWithDetailsAsync(int flightCrewId)
        {
            return await _dbSet
                .Include(fc => fc.Flight)
                    .ThenInclude(f => f.Aircraft)
                .Include(fc => fc.Pilot)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(fc => fc.FlightCrewId == flightCrewId);
        }

        public async Task<bool> IsPilotAssignedToFlightAsync(int flightId, int pilotId)
        {
            return await _dbSet.AnyAsync(fc =>
                fc.FlightId == flightId &&
                fc.PilotId == pilotId &&
                fc.IsActive);
        }

        public async Task RemoveCrewFromFlightAsync(int flightId, int pilotId)
        {
            var flightCrew = await _dbSet
                .FirstOrDefaultAsync(fc => fc.FlightId == flightId && fc.PilotId == pilotId);

            if (flightCrew != null)
            {
                await DeleteAsync(flightCrew);
            }
        }
    }
}