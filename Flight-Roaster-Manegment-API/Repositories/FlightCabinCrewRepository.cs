using FlightRosterAPI.Data;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightRosterAPI.Repositories
{
    public class FlightCabinCrewRepository : Repository<FlightCabinCrew>, IFlightCabinCrewRepository
    {
        public FlightCabinCrewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<FlightCabinCrew>> GetCabinCrewByFlightAsync(int flightId)
        {
            return await _dbSet
                .Include(fcc => fcc.CabinCrew)
                    .ThenInclude(cc => cc.User)
                .Include(fcc => fcc.Flight)
                .Where(fcc => fcc.FlightId == flightId && fcc.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<FlightCabinCrew>> GetFlightsByCabinCrewAsync(int cabinCrewId)
        {
            return await _dbSet
                .Include(fcc => fcc.Flight)
                    .ThenInclude(f => f.Aircraft)
                .Include(fcc => fcc.CabinCrew)
                    .ThenInclude(cc => cc.User)
                .Where(fcc => fcc.CabinCrewId == cabinCrewId && fcc.IsActive)
                .OrderByDescending(fcc => fcc.Flight.DepartureTime)
                .ToListAsync();
        }

        public async Task<FlightCabinCrew?> GetFlightCabinCrewWithDetailsAsync(int flightCabinCrewId)
        {
            return await _dbSet
                .Include(fcc => fcc.Flight)
                    .ThenInclude(f => f.Aircraft)
                .Include(fcc => fcc.CabinCrew)
                    .ThenInclude(cc => cc.User)
                .FirstOrDefaultAsync(fcc => fcc.FlightCabinCrewId == flightCabinCrewId);
        }

        public async Task<bool> IsCabinCrewAssignedToFlightAsync(int flightId, int cabinCrewId)
        {
            return await _dbSet.AnyAsync(fcc =>
                fcc.FlightId == flightId &&
                fcc.CabinCrewId == cabinCrewId &&
                fcc.IsActive);
        }

        public async Task RemoveCabinCrewFromFlightAsync(int flightId, int cabinCrewId)
        {
            var flightCabinCrew = await _dbSet
                .FirstOrDefaultAsync(fcc => fcc.FlightId == flightId && fcc.CabinCrewId == cabinCrewId);

            if (flightCabinCrew != null)
            {
                await DeleteAsync(flightCabinCrew);
            }
        }
    }
}