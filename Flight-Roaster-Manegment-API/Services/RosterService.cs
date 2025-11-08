using FlightRosterAPI.Models.DTOs.Roster;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Models.Enums;
using FlightRosterAPI.Repositories.Interfaces;
using FlightRosterAPI.Services.IServices;
using System.Text.Json;

namespace FlightRosterAPI.Services
{
    public class RosterService : IRosterService
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IPilotRepository _pilotRepository;
        private readonly ICabinCrewRepository _cabinCrewRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IFlightCrewRepository _flightCrewRepository;
        private readonly IFlightCabinCrewRepository _flightCabinCrewRepository;
        private readonly ISeatRepository _seatRepository;

        public RosterService(
            IFlightRepository flightRepository,
            IPilotRepository pilotRepository,
            ICabinCrewRepository cabinCrewRepository,
            IPassengerRepository passengerRepository,
            IFlightCrewRepository flightCrewRepository,
            IFlightCabinCrewRepository flightCabinCrewRepository,
            ISeatRepository seatRepository)
        {
            _flightRepository = flightRepository;
            _pilotRepository = pilotRepository;
            _cabinCrewRepository = cabinCrewRepository;
            _passengerRepository = passengerRepository;
            _flightCrewRepository = flightCrewRepository;
            _flightCabinCrewRepository = flightCabinCrewRepository;
            _seatRepository = seatRepository;
        }

        #region Roster Retrieval

        public async Task<FlightRosterResponseDto?> GetFlightRosterAsync(int flightId)
        {
            var flight = await _flightRepository.GetFlightWithDetailsAsync(flightId);
            if (flight == null)
                return null;

            return await BuildFlightRosterResponseAsync(flight);
        }

        public async Task<FlightRosterResponseDto?> GetFlightRosterByFlightNumberAsync(string flightNumber)
        {
            var flight = await _flightRepository.GetByFlightNumberAsync(flightNumber);
            if (flight == null)
                return null;

            flight = await _flightRepository.GetFlightWithDetailsAsync(flight.FlightId);
            return await BuildFlightRosterResponseAsync(flight!);
        }

        #endregion

        #region Manual Assignment

        public async Task<FlightCrewResponseDto> AssignPilotToFlightAsync(AssignFlightCrewDto assignDto)
        {
            // Validate flight exists
            var flight = await _flightRepository.GetByIdAsync(assignDto.FlightId);
            if (flight == null)
                throw new KeyNotFoundException("Uçuş bulunamadı");

            // Validate pilot exists and is available
            var pilot = await _pilotRepository.GetPilotWithUserAsync(assignDto.PilotId);
            if (pilot == null)
                throw new KeyNotFoundException("Pilot bulunamadı");

            if (!pilot.IsActive || !pilot.User.IsActive)
                throw new InvalidOperationException("Pilot aktif değil");

            // Check if pilot is already assigned
            var isAssigned = await _flightCrewRepository.IsPilotAssignedToFlightAsync(
                assignDto.FlightId, assignDto.PilotId);
            if (isAssigned)
                throw new InvalidOperationException("Pilot zaten bu uçuşa atanmış");

            // Validate pilot qualifications
            var aircraft = flight.Aircraft ?? await _flightRepository.GetFlightWithDetailsAsync(assignDto.FlightId)
                .ContinueWith(t => t.Result!.Aircraft);

            if (!pilot.QualifiedAircraftTypes.Contains(aircraft!.AircraftType))
                throw new InvalidOperationException("Pilot bu uçak tipine yetkili değil");

            if (pilot.MaxFlightDistanceKm < flight.DistanceKm)
                throw new InvalidOperationException("Pilot bu mesafeye yetkili değil");

            if (pilot.LicenseExpiryDate <= DateTime.UtcNow)
                throw new InvalidOperationException("Pilot lisansı geçersiz");

            // Create assignment
            var flightCrew = new FlightCrew
            {
                FlightId = assignDto.FlightId,
                PilotId = assignDto.PilotId,
                Role = assignDto.Role,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _flightCrewRepository.AddAsync(flightCrew);

            return new FlightCrewResponseDto
            {
                FlightCrewId = flightCrew.FlightCrewId,
                FlightId = flightCrew.FlightId,
                PilotId = flightCrew.PilotId,
                Role = flightCrew.Role,
                AssignedAt = flightCrew.AssignedAt,
                IsActive = flightCrew.IsActive
            };
        }

        public async Task<FlightCabinCrewResponseDto> AssignCabinCrewToFlightAsync(AssignCabinCrewDto assignDto)
        {
            // Validate flight exists
            var flight = await _flightRepository.GetByIdAsync(assignDto.FlightId);
            if (flight == null)
                throw new KeyNotFoundException("Uçuş bulunamadı");

            // Validate cabin crew exists
            var cabinCrew = await _cabinCrewRepository.GetCabinCrewWithUserAsync(assignDto.CabinCrewId);
            if (cabinCrew == null)
                throw new KeyNotFoundException("Kabin ekibi bulunamadı");

            if (!cabinCrew.IsActive || !cabinCrew.User.IsActive)
                throw new InvalidOperationException("Kabin ekibi aktif değil");

            // Check if already assigned
            var isAssigned = await _flightCabinCrewRepository.IsCabinCrewAssignedToFlightAsync(
                assignDto.FlightId, assignDto.CabinCrewId);
            if (isAssigned)
                throw new InvalidOperationException("Kabin ekibi zaten bu uçuşa atanmış");

            // Validate qualifications
            var aircraft = flight.Aircraft ?? await _flightRepository.GetFlightWithDetailsAsync(assignDto.FlightId)
                .ContinueWith(t => t.Result!.Aircraft);

            if (!cabinCrew.QualifiedAircraftTypes.Contains(aircraft!.AircraftType))
                throw new InvalidOperationException("Kabin ekibi bu uçak tipine yetkili değil");

            // Validate recipe for chefs
            string? assignedRecipe = null;
            if (cabinCrew.CrewType == CabinCrewType.Chef)
            {
                if (!string.IsNullOrEmpty(assignDto.AssignedRecipe))
                {
                    var recipes = cabinCrew.Recipes?.Split(',').Select(r => r.Trim()).ToList() ?? new List<string>();
                    if (!recipes.Contains(assignDto.AssignedRecipe))
                        throw new InvalidOperationException("Seçilen tarif aşçının repertuarında yok");
                    assignedRecipe = assignDto.AssignedRecipe;
                }
            }

            // Create assignment
            var flightCabinCrew = new FlightCabinCrew
            {
                FlightId = assignDto.FlightId,
                CabinCrewId = assignDto.CabinCrewId,
                AssignedRecipe = assignedRecipe,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _flightCabinCrewRepository.AddAsync(flightCabinCrew);

            return new FlightCabinCrewResponseDto
            {
                FlightCabinCrewId = flightCabinCrew.FlightCabinCrewId,
                FlightId = flightCabinCrew.FlightId,
                CabinCrewId = flightCabinCrew.CabinCrewId,
                AssignedRecipe = flightCabinCrew.AssignedRecipe,
                AssignedAt = flightCabinCrew.AssignedAt,
                IsActive = flightCabinCrew.IsActive
            };
        }

        public async Task<bool> RemovePilotFromFlightAsync(int flightId, int pilotId)
        {
            await _flightCrewRepository.RemoveCrewFromFlightAsync(flightId, pilotId);
            return true;
        }

        public async Task<bool> RemoveCabinCrewFromFlightAsync(int flightId, int cabinCrewId)
        {
            await _flightCabinCrewRepository.RemoveCabinCrewFromFlightAsync(flightId, cabinCrewId);
            return true;
        }

        #endregion

        #region Auto Assignment

        public async Task<FlightRosterResponseDto> AutoAssignCrewAsync(AutoAssignCrewDto autoAssignDto)
        {
            var flight = await _flightRepository.GetFlightWithDetailsAsync(autoAssignDto.FlightId);
            if (flight == null)
                throw new KeyNotFoundException("Uçuş bulunamadı");

            // Auto-assign pilots
            if (autoAssignDto.AssignPilots)
            {
                await AutoAssignPilotsAsync(flight);
            }

            // Auto-assign cabin crew
            if (autoAssignDto.AssignCabinCrew)
            {
                await AutoAssignCabinCrewAsync(flight);
            }

            // Reload flight with updated crew
            flight = await _flightRepository.GetFlightWithDetailsAsync(autoAssignDto.FlightId);
            return await BuildFlightRosterResponseAsync(flight!);
        }

        private async Task AutoAssignPilotsAsync(Flight flight)
        {
            var aircraft = flight.Aircraft;
            var availablePilots = (await _pilotRepository.GetAvailablePilotsForFlightAsync(
                flight.AircraftId, flight.DistanceKm)).ToList();

            if (!availablePilots.Any())
                throw new InvalidOperationException("Uygun pilot bulunamadı");

            // Separate by seniority
            var seniorPilots = availablePilots.Where(p => p.Seniority == PilotSeniority.Senior).ToList();
            var juniorPilots = availablePilots.Where(p => p.Seniority == PilotSeniority.Junior).ToList();
            var traineePilots = availablePilots.Where(p => p.Seniority == PilotSeniority.Trainee).ToList();

            // Validation: Need at least 1 senior and 1 junior
            if (!seniorPilots.Any())
                throw new InvalidOperationException("En az 1 senior pilot gerekli");

            if (!juniorPilots.Any())
                throw new InvalidOperationException("En az 1 junior pilot gerekli");

            var assignedPilots = new List<Pilot>();

            // Assign 1 senior as Captain
            var captain = seniorPilots.OrderByDescending(p => p.TotalFlightHours).First();
            assignedPilots.Add(captain);
            await AssignPilotToFlightAsync(new AssignFlightCrewDto
            {
                FlightId = flight.FlightId,
                PilotId = captain.PilotId,
                Role = "Captain"
            });

            // Assign 1 junior as First Officer
            var firstOfficer = juniorPilots.OrderByDescending(p => p.TotalFlightHours).First();
            assignedPilots.Add(firstOfficer);
            await AssignPilotToFlightAsync(new AssignFlightCrewDto
            {
                FlightId = flight.FlightId,
                PilotId = firstOfficer.PilotId,
                Role = "First Officer"
            });

            // Assign up to 2 trainees if available and crew capacity allows
            int currentCrew = assignedPilots.Count;
            int maxTrainees = Math.Min(2, Math.Min(traineePilots.Count, aircraft.MaxCrewCapacity - currentCrew));

            for (int i = 0; i < maxTrainees; i++)
            {
                var trainee = traineePilots[i];
                await AssignPilotToFlightAsync(new AssignFlightCrewDto
                {
                    FlightId = flight.FlightId,
                    PilotId = trainee.PilotId,
                    Role = "Trainee"
                });
            }
        }

        private async Task AutoAssignCabinCrewAsync(Flight flight)
        {
            var aircraft = flight.Aircraft;
            var availableCabinCrew = (await _cabinCrewRepository.GetAvailableCabinCrewForFlightAsync(
                aircraft.AircraftType)).ToList();

            if (!availableCabinCrew.Any())
                throw new InvalidOperationException("Uygun kabin ekibi bulunamadı");

            // Separate by type and seniority
            var seniorRegular = availableCabinCrew
                .Where(c => c.CrewType == CabinCrewType.Regular && c.Seniority == CabinCrewSeniority.Senior)
                .ToList();
            var juniorRegular = availableCabinCrew
                .Where(c => c.CrewType == CabinCrewType.Regular && c.Seniority == CabinCrewSeniority.Junior)
                .ToList();
            var chiefs = availableCabinCrew.Where(c => c.CrewType == CabinCrewType.Chief).ToList();
            var chefs = availableCabinCrew.Where(c => c.CrewType == CabinCrewType.Chef).ToList();

            var assignedCrew = new List<CabinCrew>();

            // Assign 1-4 senior crew (including 1 chief if available)
            int seniorCount = Math.Min(4, Math.Max(1, seniorRegular.Count + chiefs.Count));

            // Assign chief first
            if (chiefs.Any())
            {
                var chief = chiefs.First();
                assignedCrew.Add(chief);
                await AssignCabinCrewToFlightAsync(new AssignCabinCrewDto
                {
                    FlightId = flight.FlightId,
                    CabinCrewId = chief.CabinCrewId
                });
                seniorCount--;
            }

            // Assign remaining senior crew
            for (int i = 0; i < Math.Min(seniorCount, seniorRegular.Count); i++)
            {
                var crew = seniorRegular[i];
                assignedCrew.Add(crew);
                await AssignCabinCrewToFlightAsync(new AssignCabinCrewDto
                {
                    FlightId = flight.FlightId,
                    CabinCrewId = crew.CabinCrewId
                });
            }

            // Assign 4-16 junior crew
            int juniorCount = Math.Min(16, Math.Max(4, juniorRegular.Count));
            juniorCount = Math.Min(juniorCount, aircraft.MaxCabinCrewCapacity - assignedCrew.Count);

            for (int i = 0; i < Math.Min(juniorCount, juniorRegular.Count); i++)
            {
                var crew = juniorRegular[i];
                assignedCrew.Add(crew);
                await AssignCabinCrewToFlightAsync(new AssignCabinCrewDto
                {
                    FlightId = flight.FlightId,
                    CabinCrewId = crew.CabinCrewId
                });
            }

            // Assign 0-2 chefs with random recipes
            int chefCount = Math.Min(2, Math.Min(chefs.Count, aircraft.MaxCabinCrewCapacity - assignedCrew.Count));
            var random = new Random();

            for (int i = 0; i < chefCount; i++)
            {
                var chef = chefs[i];
                var recipes = chef.Recipes?.Split(',').Select(r => r.Trim()).ToList() ?? new List<string>();
                var selectedRecipe = recipes.Any() ? recipes[random.Next(recipes.Count)] : null;

                assignedCrew.Add(chef);
                await AssignCabinCrewToFlightAsync(new AssignCabinCrewDto
                {
                    FlightId = flight.FlightId,
                    CabinCrewId = chef.CabinCrewId,
                    AssignedRecipe = selectedRecipe
                });
            }
        }

        public async Task<bool> ValidateFlightCrewAsync(int flightId)
        {
            var flight = await _flightRepository.GetFlightWithDetailsAsync(flightId);
            if (flight == null)
                return false;

            var pilots = flight.FlightCrews.Where(fc => fc.IsActive).ToList();
            var cabinCrew = flight.FlightCabinCrews.Where(fcc => fcc.IsActive).ToList();

            // Validate pilot requirements
            var seniorPilots = pilots.Count(p => p.Pilot.Seniority == PilotSeniority.Senior);
            var juniorPilots = pilots.Count(p => p.Pilot.Seniority == PilotSeniority.Junior);
            var trainees = pilots.Count(p => p.Pilot.Seniority == PilotSeniority.Trainee);

            if (seniorPilots < 1 || juniorPilots < 1 || trainees > 2)
                return false;

            // Validate cabin crew requirements
            var seniorCabin = cabinCrew.Count(c => c.CabinCrew.Seniority == CabinCrewSeniority.Senior);
            var juniorCabin = cabinCrew.Count(c => c.CabinCrew.Seniority == CabinCrewSeniority.Junior);
            var chefs = cabinCrew.Count(c => c.CabinCrew.CrewType == CabinCrewType.Chef);

            if (seniorCabin < 1 || seniorCabin > 4 || juniorCabin < 4 || juniorCabin > 16 || chefs > 2)
                return false;

            return true;
        }

        #endregion

        #region Export

        public async Task<string> ExportRosterToJsonAsync(int flightId)
        {
            var roster = await GetFlightRosterAsync(flightId);
            if (roster == null)
                throw new KeyNotFoundException("Uçuş bulunamadı");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(roster, options);
        }

        public async Task<byte[]> ExportRosterToPdfAsync(int flightId)
        {
            // TODO: Implement PDF export using a library like QuestPDF or iTextSharp
            throw new NotImplementedException("PDF export özelliği henüz eklenmedi");
        }

        #endregion

        #region Private Helpers

        private async Task<FlightRosterResponseDto> BuildFlightRosterResponseAsync(Flight flight)
        {
            var pilots = flight.FlightCrews.Where(fc => fc.IsActive).Select(fc => new RosterPilotDto
            {
                PilotId = fc.Pilot.PilotId,
                FullName = fc.Pilot.User.FullName,
                Role = fc.Role,
                Seniority = fc.Pilot.Seniority.ToString(),
                LicenseNumber = fc.Pilot.LicenseNumber,
                TotalFlightHours = fc.Pilot.TotalFlightHours
            }).ToList();

            var cabinCrew = flight.FlightCabinCrews.Where(fcc => fcc.IsActive).Select(fcc => new RosterCabinCrewDto
            {
                CabinCrewId = fcc.CabinCrew.CabinCrewId,
                FullName = fcc.CabinCrew.User.FullName,
                CrewType = fcc.CabinCrew.CrewType.ToString(),
                Seniority = fcc.CabinCrew.Seniority.ToString(),
                AssignedRecipe = fcc.AssignedRecipe,
                Languages = fcc.CabinCrew.Languages?.Split(',').Select(l => l.Trim()).ToList()
            }).ToList();

            var passengers = flight.Seats.Where(s => s.Passenger != null).Select(s => new RosterPassengerDto
            {
                PassengerId = s.Passenger!.PassengerId,
                FullName = s.Passenger.User.FullName,
                Age = CalculateAge(s.Passenger.User.DateOfBirth),
                Nationality = s.Passenger.User.Nationality,
                SeatNumber = s.SeatNumber,
                SeatClass = s.SeatClass.ToString(),
                IsInfant = s.IsInfantSeat,
                ParentName = s.ParentPassenger?.User.FullName
            }).ToList();

            var summary = new FlightCrewSummary
            {
                TotalPilots = pilots.Count,
                SeniorPilots = pilots.Count(p => p.Seniority == "Senior"),
                JuniorPilots = pilots.Count(p => p.Seniority == "Junior"),
                TraineePilots = pilots.Count(p => p.Seniority == "Trainee"),
                TotalCabinCrew = cabinCrew.Count,
                SeniorCabinCrew = cabinCrew.Count(c => c.Seniority == "Senior"),
                JuniorCabinCrew = cabinCrew.Count(c => c.Seniority == "Junior"),
                Chefs = cabinCrew.Count(c => c.CrewType == "Chef"),
                TotalPassengers = passengers.Count,
                BusinessPassengers = passengers.Count(p => p.SeatClass == "Business"),
                EconomyPassengers = passengers.Count(p => p.SeatClass == "Economy"),
                Infants = passengers.Count(p => p.IsInfant)
            };

            return new FlightRosterResponseDto
            {
                FlightId = flight.FlightId,
                FlightNumber = flight.FlightNumber,
                AircraftType = flight.Aircraft.AircraftType,
                DepartureTime = flight.DepartureTime,
                DepartureAirport = $"{flight.DepartureAirportCode} - {flight.DepartureAirport}",
                ArrivalAirport = $"{flight.ArrivalAirportCode} - {flight.ArrivalAirport}",
                CrewSummary = summary,
                Pilots = pilots,
                CabinCrew = cabinCrew,
                Passengers = passengers
            };
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        #endregion
    }
}