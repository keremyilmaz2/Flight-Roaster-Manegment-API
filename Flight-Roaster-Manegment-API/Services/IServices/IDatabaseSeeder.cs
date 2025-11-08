using FlightRosterAPI.Data;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Bogus;

namespace FlightRosterAPI.Services.IServices
{
    public interface IDatabaseSeeder
    {
        Task<bool> HasDataAsync();
        Task SeedTestDataAsync();
    }
}