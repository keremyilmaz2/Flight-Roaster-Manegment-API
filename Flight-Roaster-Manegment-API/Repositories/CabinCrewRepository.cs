using FlightRosterAPI.Data;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Models.Enums;
using FlightRosterAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightRosterAPI.Repositories
{
    public class CabinCrewRepository : Repository<CabinCrew>, ICabinCrewRepository
    {
        public CabinCrewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<CabinCrew?> GetCabinCrewWithUserAsync(int cabinCrewId)
        {
            return await _dbSet
                .Include(cc => cc.User)
                .FirstOrDefaultAsync(cc => cc.CabinCrewId == cabinCrewId);
        }

        public async Task<CabinCrew?> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(cc => cc.User)
                .FirstOrDefaultAsync(cc => cc.UserId == userId);
        }

        public async Task<IEnumerable<CabinCrew>> GetActiveCabinCrewAsync()
        {
            return await _dbSet
                .Include(cc => cc.User)
                .Where(cc => cc.IsActive && cc.User.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<CabinCrew>> GetCabinCrewByTypeAsync(CabinCrewType crewType)
        {
            return await _dbSet
                .Include(cc => cc.User)
                .Where(cc => cc.CrewType == crewType && cc.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<CabinCrew>> GetCabinCrewBySeniorityAsync(CabinCrewSeniority seniority)
        {
            return await _dbSet
                .Include(cc => cc.User)
                .Where(cc => cc.Seniority == seniority && cc.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<CabinCrew>> GetAvailableCabinCrewForFlightAsync(string aircraftType)
        {
            return await _dbSet
                .Include(cc => cc.User)
                .Where(cc => cc.IsActive &&
                           cc.User.IsActive &&
                           cc.QualifiedAircraftTypes.Contains(aircraftType))
                .ToListAsync();
        }

        public async Task<IEnumerable<CabinCrew>> GetChefsWithRecipesAsync()
        {
            return await _dbSet
                .Include(cc => cc.User)
                .Where(cc => cc.CrewType == CabinCrewType.Chef &&
                           cc.IsActive &&
                           !string.IsNullOrEmpty(cc.Recipes))
                .ToListAsync();
        }
    }
}