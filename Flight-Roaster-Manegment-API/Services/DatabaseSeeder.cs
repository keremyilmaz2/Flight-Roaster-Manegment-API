using Bogus;
using FlightRosterAPI.Data;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Models.Enums;
using FlightRosterAPI.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;



namespace FlightRosterAPI.Services
{
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<bool> HasDataAsync()
        {
            return await _context.Aircrafts.AnyAsync() ||
                   await _context.Flights.AnyAsync() ||
                   await _context.Pilots.AnyAsync();
        }

        public async Task SeedTestDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting to seed test data...");

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Rolleri oluştur
                    await EnsureRolesExistAsync();

                    // Admin kullanıcısı oluştur
                    var adminUser = await CreateAdminUserAsync();

                    // Test kullanıcıları oluştur
                    var pilotUsers = await CreateTestUsersAsync(20, "pilot", UserRole.Pilot);
                    var cabinCrewUsers = await CreateTestUsersAsync(50, "cabincrew", UserRole.CabinCrew);
                    var passengerUsers = await CreateTestUsersAsync(100, "passenger", UserRole.Passenger);

                    // Uçakları oluştur
                    var aircrafts = await CreateAircraftsAsync();

                    // Pilotları oluştur
                    await CreatePilotsAsync(pilotUsers, aircrafts);

                    // Kabin ekibini oluştur
                    await CreateCabinCrewAsync(cabinCrewUsers, aircrafts);

                    // Yolcuları oluştur
                    await CreatePassengersAsync(passengerUsers);

                    // Uçuşları oluştur
                    var flights = await CreateFlightsAsync(aircrafts);

                    // Koltukları oluştur
                    await CreateSeatsAsync(flights);

                    // Uçuş ekiplerini ata
                    await AssignFlightCrewsAsync(flights);

                    // Uçuş kabin ekiplerini ata
                    await AssignFlightCabinCrewsAsync(flights);

                    // Yolcuları koltuklara ata
                    await AssignPassengersToSeatsAsync(flights);

                    await transaction.CommitAsync();

                    _logger.LogInformation("Test data seeded successfully!");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while seeding data. Transaction rolled back.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to seed test data");
                throw;
            }
        }

        private async Task EnsureRolesExistAsync()
        {
            string[] roles = { "Admin", "Pilot", "CabinCrew", "Passenger" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole<int>(role));
                    _logger.LogInformation($"Created role: {role}");
                }
            }
        }

        private async Task<User> CreateAdminUserAsync()
        {
            var adminEmail = "admin@flightroster.com";
            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin != null)
            {
                _logger.LogInformation("Admin user already exists");
                return existingAdmin;
            }

            var admin = new User
            {
                UserName = "admin",
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                Role = UserRole.Admin,
                DateOfBirth = new DateTime(1980, 1, 1),
                Nationality = "Türkiye",
                PhoneNumber = "+905551234567",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(admin, "Admin123!");

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, "Admin");
                _logger.LogInformation("Created admin user");
                return admin;
            }

            throw new Exception("Failed to create admin user");
        }

        private async Task<List<User>> CreateTestUsersAsync(int count, string prefix, UserRole role)
        {
            var faker = new Faker<User>("tr")
                .RuleFor(u => u.UserName, f => $"{prefix}_{f.Internet.UserName()}")
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber("+90##########"))
                .RuleFor(u => u.DateOfBirth, f => f.Date.Past(50, DateTime.Now.AddYears(-18)))
                .RuleFor(u => u.Gender, f => "Erkek")
                .RuleFor(u => u.Nationality, f => "Türkiye")
                .RuleFor(u => u.Role, f => role)
                .RuleFor(u => u.EmailConfirmed, f => true)
                .RuleFor(u => u.IsActive, f => true)
                .RuleFor(u => u.CreatedAt, f => DateTime.UtcNow);

            var users = faker.Generate(count);
            var createdUsers = new List<User>();

            string roleName = role.ToString();

            foreach (var user in users)
            {
                var result = await _userManager.CreateAsync(user, "Test123!");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                    createdUsers.Add(user);
                }
            }

            _logger.LogInformation($"Created {createdUsers.Count} users with role {roleName}");
            return createdUsers;
        }

        private async Task<List<Aircraft>> CreateAircraftsAsync()
        {
            var aircraftTypes = new[]
            {
                new { Type = "Boeing 737-800", TotalSeats = 189, Business = 16, Economy = 173, MinCrew = 2, MaxCrew = 4, MinCabin = 4, MaxCabin = 6, Range = 5436 },
                new { Type = "Airbus A320", TotalSeats = 180, Business = 12, Economy = 168, MinCrew = 2, MaxCrew = 4, MinCabin = 4, MaxCabin = 6, Range = 5700 },
                new { Type = "Boeing 777-300ER", TotalSeats = 396, Business = 42, Economy = 354, MinCrew = 2, MaxCrew = 4, MinCabin = 10, MaxCabin = 14, Range = 13649 },
                new { Type = "Airbus A330-300", TotalSeats = 300, Business = 30, Economy = 270, MinCrew = 2, MaxCrew = 4, MinCabin = 8, MaxCabin = 12, Range = 10400 },
                new { Type = "Boeing 787-9", TotalSeats = 290, Business = 30, Economy = 260, MinCrew = 2, MaxCrew = 4, MinCabin = 8, MaxCabin = 10, Range = 14140 }
            };

            var aircrafts = new List<Aircraft>();
            var faker = new Faker();

            foreach (var type in aircraftTypes)
            {
                for (int i = 0; i < 3; i++)
                {
                    var aircraft = new Aircraft
                    {
                        AircraftType = type.Type,
                        RegistrationNumber = $"TC-{faker.Random.String2(3, "ABCDEFGHIJKLMNOPQRSTUVWXYZ")}{faker.Random.Int(10, 99)}",
                        TotalSeats = type.TotalSeats,
                        BusinessClassSeats = type.Business,
                        EconomyClassSeats = type.Economy,
                        MinCrewRequired = type.MinCrew,
                        MaxCrewCapacity = type.MaxCrew,
                        MinCabinCrewRequired = type.MinCabin,
                        MaxCabinCrewCapacity = type.MaxCabin,
                        MaxRangeKm = type.Range,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    aircrafts.Add(aircraft);
                }
            }

            await _context.Aircrafts.AddRangeAsync(aircrafts);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created {aircrafts.Count} aircrafts");
            return aircrafts;
        }

        private async Task CreatePilotsAsync(List<User> users, List<Aircraft> aircrafts)
        {
            var faker = new Faker();
            var pilots = new List<Pilot>();

            foreach (var user in users)
            {
                var seniority = faker.PickRandom<PilotSeniority>();
                var qualifiedTypes = faker.PickRandom(aircrafts.Select(a => a.AircraftType).Distinct().ToList(), faker.Random.Int(1, 3));

                var pilot = new Pilot
                {
                    UserId = user.Id,
                    LicenseNumber = $"PL{faker.Random.Int(100000, 999999)}",
                    Seniority = seniority,
                    MaxFlightDistanceKm = seniority switch
                    {
                        PilotSeniority.Trainee => faker.Random.Double(1000, 3000),
                        PilotSeniority.Junior => faker.Random.Double(3000, 8000),
                        PilotSeniority.Senior => faker.Random.Double(8000, 15000),
                        _ => 5000
                    },
                    QualifiedAircraftTypes = string.Join(",", qualifiedTypes),
                    TotalFlightHours = seniority switch
                    {
                        PilotSeniority.Trainee => faker.Random.Int(100, 500),
                        PilotSeniority.Junior => faker.Random.Int(500, 2000),
                        PilotSeniority.Senior => faker.Random.Int(2000, 10000),
                        _ => 1000
                    },
                    LicenseExpiryDate = faker.Date.Future(2),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                pilots.Add(pilot);
            }

            await _context.Pilots.AddRangeAsync(pilots);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created {pilots.Count} pilots");
        }

        private async Task CreateCabinCrewAsync(List<User> users, List<Aircraft> aircrafts)
        {
            var faker = new Faker();
            var cabinCrews = new List<CabinCrew>();

            var recipes = new[] { "İtalyan Mutfağı", "Türk Mutfağı", "Uzak Doğu Mutfağı", "Akdeniz Mutfağı" };
            var languages = new[] { "İngilizce", "Almanca", "Fransızca", "İspanyolca", "Arapça" };

            foreach (var user in users)
            {
                var crewType = faker.PickRandom<CabinCrewType>();
                var seniority = faker.PickRandom<CabinCrewSeniority>();
                var qualifiedTypes = faker.PickRandom(aircrafts.Select(a => a.AircraftType).Distinct().ToList(), faker.Random.Int(1, 3));

                var cabinCrew = new CabinCrew
                {
                    UserId = user.Id,
                    CrewType = crewType,
                    Seniority = seniority,
                    QualifiedAircraftTypes = string.Join(",", qualifiedTypes),
                    Recipes = crewType == CabinCrewType.Chef ? string.Join(",", faker.PickRandom(recipes, faker.Random.Int(2, 4))) : null,
                    Languages = string.Join(",", faker.PickRandom(languages, faker.Random.Int(1, 3))),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                cabinCrews.Add(cabinCrew);
            }

            await _context.CabinCrews.AddRangeAsync(cabinCrews);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created {cabinCrews.Count} cabin crew members");
        }

        private async Task CreatePassengersAsync(List<User> users)
        {
            var faker = new Faker();
            var passengers = new List<Passenger>();

            foreach (var user in users)
            {
                var passenger = new Passenger
                {
                    UserId = user.Id,
                    PassportNumber = $"T{faker.Random.Int(10000000, 99999999)}",
                    NationalIdNumber = faker.Random.String2(11, "0123456789"),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                passengers.Add(passenger);
            }

            await _context.Passengers.AddRangeAsync(passengers);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created {passengers.Count} passengers");
        }

        private async Task<List<Flight>> CreateFlightsAsync(List<Aircraft> aircrafts)
        {
            var faker = new Faker();
            var flights = new List<Flight>();

            var routes = new[]
            {
                new { From = ("İstanbul", "İstanbul", "Atatürk Havalimanı", "IST", "Türkiye"), To = ("New York", "New York", "JFK Airport", "JFK", "ABD"), Distance = 8050.0 },
                new { From = ("İstanbul", "İstanbul", "Sabiha Gökçen", "SAW", "Türkiye"), To = ("Londra", "Londra", "Heathrow", "LHR", "İngiltere"), Distance = 2500.0 },
                new { From = ("Ankara", "Ankara", "Esenboğa", "ESB", "Türkiye"), To = ("Berlin", "Berlin", "Brandenburg", "BER", "Almanya"), Distance = 2100.0 },
                new { From = ("İzmir", "İzmir", "Adnan Menderes", "ADB", "Türkiye"), To = ("Paris", "Paris", "Charles de Gaulle", "CDG", "Fransa"), Distance = 2300.0 },
                new { From = ("Antalya", "Antalya", "Antalya Havalimanı", "AYT", "Türkiye"), To = ("Dubai", "Dubai", "Dubai International", "DXB", "BAE"), Distance = 2800.0 }
            };

            for (int i = 0; i < 30; i++)
            {
                var route = faker.PickRandom(routes);
                var aircraft = faker.PickRandom(aircrafts);
                var departureTime = faker.Date.Between(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(60));
                var duration = (int)(route.Distance / 800 * 60); // Yaklaşık uçuş süresi
                var arrivalTime = departureTime.AddMinutes(duration);

                var flight = new Flight
                {
                    FlightNumber = $"TK{faker.Random.Int(100, 999)}",
                    AircraftId = aircraft.AircraftId,
                    DepartureCountry = route.From.Item5,
                    DepartureCity = route.From.Item1,
                    DepartureAirport = route.From.Item3,
                    DepartureAirportCode = route.From.Item4,
                    ArrivalCountry = route.To.Item5,
                    ArrivalCity = route.To.Item1,
                    ArrivalAirport = route.To.Item3,
                    ArrivalAirportCode = route.To.Item4,
                    DepartureTime = departureTime,
                    ArrivalTime = arrivalTime,
                    DurationMinutes = duration,
                    DistanceKm = route.Distance,
                    CodeShareFlightNumber = faker.Random.Bool(0.3f) ? $"LH{faker.Random.Int(1000, 9999)}" : null,
                    CodeShareAirline = faker.Random.Bool(0.3f) ? "Lufthansa" : null,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                flights.Add(flight);
            }

            await _context.Flights.AddRangeAsync(flights);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created {flights.Count} flights");
            return flights;
        }

        private async Task CreateSeatsAsync(List<Flight> flights)
        {
            var totalSeatsCreated = 0;

            foreach (var flight in flights)
            {
                var aircraft = await _context.Aircrafts.FindAsync(flight.AircraftId);
                if (aircraft == null) continue;

                var seats = new List<Seat>();

                // Business class koltukları (4'erli dizilim)
                int seatNumber = 1;
                for (int i = 0; i < aircraft.BusinessClassSeats; i++)
                {
                    var row = (i / 4) + 1;
                    var col = (char)('A' + (i % 4));

                    seats.Add(new Seat
                    {
                        FlightId = flight.FlightId,
                        SeatNumber = $"{row}{col}",
                        SeatClass = SeatClass.Business,
                        IsOccupied = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // Economy class koltukları (6'lı dizilim)
                int economyStartRow = (aircraft.BusinessClassSeats / 4) + 2;
                for (int i = 0; i < aircraft.EconomyClassSeats; i++)
                {
                    var row = economyStartRow + (i / 6);
                    var col = (char)('A' + (i % 6));

                    seats.Add(new Seat
                    {
                        FlightId = flight.FlightId,
                        SeatNumber = $"{row}{col}",
                        SeatClass = SeatClass.Economy,
                        IsOccupied = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.Seats.AddRangeAsync(seats);
                totalSeatsCreated += seats.Count;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Created {totalSeatsCreated} seats for {flights.Count} flights");
        }

        private async Task AssignFlightCrewsAsync(List<Flight> flights)
        {
            var faker = new Faker();
            var totalAssignments = 0;
            var assignedFlights = 0;

            foreach (var flight in flights)
            {
                var aircraft = await _context.Aircrafts.FindAsync(flight.AircraftId);
                if (aircraft == null) continue;

                var availablePilots = await _context.Pilots
                    .Include(p => p.User)
                    .Where(p => p.IsActive &&
                               p.User.IsActive &&
                               p.MaxFlightDistanceKm >= flight.DistanceKm &&
                               p.QualifiedAircraftTypes.Contains(aircraft.AircraftType) &&
                               p.LicenseExpiryDate > DateTime.UtcNow)
                    .ToListAsync();

                var seniorPilots = availablePilots.Where(p => p.Seniority == PilotSeniority.Senior).ToList();
                var juniorPilots = availablePilots.Where(p => p.Seniority == PilotSeniority.Junior).ToList();
                var traineePilots = availablePilots.Where(p => p.Seniority == PilotSeniority.Trainee).ToList();

                if (!seniorPilots.Any() || !juniorPilots.Any())
                {
                    _logger.LogWarning($"Not enough qualified pilots for flight {flight.FlightNumber}");
                    continue;
                }

                // Captain (Senior)
                var captain = faker.PickRandom(seniorPilots);
                await _context.FlightCrews.AddAsync(new FlightCrew
                {
                    FlightId = flight.FlightId,
                    PilotId = captain.PilotId,
                    Role = "Captain",
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true
                });
                totalAssignments++;

                // First Officer (Junior)
                var firstOfficer = faker.PickRandom(juniorPilots);
                await _context.FlightCrews.AddAsync(new FlightCrew
                {
                    FlightId = flight.FlightId,
                    PilotId = firstOfficer.PilotId,
                    Role = "First Officer",
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true
                });
                totalAssignments++;

                // Opsiyonel Trainee
                if (traineePilots.Any() && faker.Random.Bool(0.3f))
                {
                    var trainee = faker.PickRandom(traineePilots);
                    await _context.FlightCrews.AddAsync(new FlightCrew
                    {
                        FlightId = flight.FlightId,
                        PilotId = trainee.PilotId,
                        Role = "Trainee",
                        AssignedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                    totalAssignments++;
                }

                assignedFlights++;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Assigned {totalAssignments} pilots to {assignedFlights} flights");
        }

        private async Task AssignFlightCabinCrewsAsync(List<Flight> flights)
        {
            var faker = new Faker();
            var totalAssignments = 0;

            foreach (var flight in flights)
            {
                var aircraft = await _context.Aircrafts.FindAsync(flight.AircraftId);
                if (aircraft == null) continue;

                var availableCabinCrew = await _context.CabinCrews
                    .Include(c => c.User)
                    .Where(c => c.IsActive &&
                               c.User.IsActive &&
                               c.QualifiedAircraftTypes.Contains(aircraft.AircraftType))
                    .ToListAsync();

                var seniorCrew = availableCabinCrew.Where(c => c.Seniority == CabinCrewSeniority.Senior).ToList();
                var juniorCrew = availableCabinCrew.Where(c => c.Seniority == CabinCrewSeniority.Junior).ToList();
                var chiefs = availableCabinCrew.Where(c => c.CrewType == CabinCrewType.Chief).ToList();
                var chefs = availableCabinCrew.Where(c => c.CrewType == CabinCrewType.Chef).ToList();

                // Chief ata
                if (chiefs.Any())
                {
                    var chief = faker.PickRandom(chiefs);
                    await _context.FlightCabinCrews.AddAsync(new FlightCabinCrew
                    {
                        FlightId = flight.FlightId,
                        CabinCrewId = chief.CabinCrewId,
                        AssignedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                    totalAssignments++;
                }

                // Senior crew ata (2-3 kişi)
                var seniorCount = faker.Random.Int(2, 3);
                foreach (var crew in faker.PickRandom(seniorCrew, Math.Min(seniorCount, seniorCrew.Count)))
                {
                    await _context.FlightCabinCrews.AddAsync(new FlightCabinCrew
                    {
                        FlightId = flight.FlightId,
                        CabinCrewId = crew.CabinCrewId,
                        AssignedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                    totalAssignments++;
                }

                // Junior crew ata (4-8 kişi)
                var juniorCount = faker.Random.Int(4, 8);
                foreach (var crew in faker.PickRandom(juniorCrew, Math.Min(juniorCount, juniorCrew.Count)))
                {
                    await _context.FlightCabinCrews.AddAsync(new FlightCabinCrew
                    {
                        FlightId = flight.FlightId,
                        CabinCrewId = crew.CabinCrewId,
                        AssignedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                    totalAssignments++;
                }

                // Chef ata (0-2 kişi)
                if (chefs.Any())
                {
                    var chefCount = faker.Random.Int(0, 2);
                    foreach (var chef in faker.PickRandom(chefs, Math.Min(chefCount, chefs.Count)))
                    {
                        var recipes = chef.Recipes?.Split(',').Select(r => r.Trim()).ToList();
                        var selectedRecipe = recipes?.Any() == true ? faker.PickRandom(recipes) : null;

                        await _context.FlightCabinCrews.AddAsync(new FlightCabinCrew
                        {
                            FlightId = flight.FlightId,
                            CabinCrewId = chef.CabinCrewId,
                            AssignedRecipe = selectedRecipe,
                            AssignedAt = DateTime.UtcNow,
                            IsActive = true
                        });
                        totalAssignments++;
                    }
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Assigned {totalAssignments} cabin crew members to flights");
        }

        private async Task AssignPassengersToSeatsAsync(List<Flight> flights)
        {
            var faker = new Faker();
            var allPassengers = await _context.Passengers.Include(p => p.User).Where(p => p.IsActive).ToListAsync();
            var totalAssignments = 0;

            if (!allPassengers.Any())
            {
                _logger.LogWarning("No passengers available for seat assignment");
                return;
            }

            foreach (var flight in flights)
            {
                var seats = await _context.Seats
                    .Where(s => s.FlightId == flight.FlightId && !s.IsOccupied)
                    .ToListAsync();

                if (!seats.Any())
                {
                    _logger.LogWarning($"No available seats for flight {flight.FlightNumber}");
                    continue;
                }

                // Koltukların %60-85'ini doldur, ama yolcu sayısından fazla olmasın
                var occupancyRate = faker.Random.Double(0.6, 0.85);
                var seatsToFill = (int)(seats.Count * occupancyRate);

                // Yolcu sayısından fazla koltuk seçme
                seatsToFill = Math.Min(seatsToFill, allPassengers.Count);
                seatsToFill = Math.Min(seatsToFill, seats.Count);

                if (seatsToFill == 0)
                {
                    _logger.LogWarning($"No seats to fill for flight {flight.FlightNumber}");
                    continue;
                }

                var selectedSeats = faker.PickRandom(seats, seatsToFill).ToList();

                // Aynı yolcuları tekrar kullanabiliriz (farklı uçuşlar için)
                var selectedPassengers = new List<Passenger>();
                for (int i = 0; i < seatsToFill; i++)
                {
                    selectedPassengers.Add(faker.PickRandom(allPassengers));
                }

                for (int i = 0; i < selectedSeats.Count; i++)
                {
                    var seat = selectedSeats[i];
                    var passenger = selectedPassengers[i];

                    seat.PassengerId = passenger.PassengerId;
                    seat.IsOccupied = true;
                    seat.BookedAt = faker.Date.Past(30);
                    seat.UpdatedAt = DateTime.UtcNow;

                    totalAssignments++;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Assigned {totalAssignments} passengers to seats across {flights.Count} flights");
        }
    }
}