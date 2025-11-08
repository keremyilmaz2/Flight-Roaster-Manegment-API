using FlightRosterAPI.Models.DTOs.Flight;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Repositories.Interfaces;
using FlightRosterAPI.Services.IServices;

namespace FlightRosterAPI.Services
{
    public class FlightService : IFlightService
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IAircraftRepository _aircraftRepository;
        private readonly ILogger<FlightService> _logger;

        public FlightService(
            IFlightRepository flightRepository,
            IAircraftRepository aircraftRepository,
            ILogger<FlightService> logger)
        {
            _flightRepository = flightRepository;
            _aircraftRepository = aircraftRepository;
            _logger = logger;
        }

        public async Task<FlightResponseDto?> GetFlightByIdAsync(int flightId)
        {
            var flight = await _flightRepository.GetByIdAsync(flightId);
            if (flight == null)
                return null;

            var aircraft = await _aircraftRepository.GetByIdAsync(flight.AircraftId);
            return MapToResponseDto(flight, aircraft!);
        }

        public async Task<FlightDetailResponseDto?> GetFlightWithDetailsAsync(int flightId)
        {
            var flight = await _flightRepository.GetFlightWithDetailsAsync(flightId);
            if (flight == null)
                return null;

            return MapToDetailResponseDto(flight);
        }

        public async Task<FlightResponseDto?> GetFlightByFlightNumberAsync(string flightNumber)
        {
            var flight = await _flightRepository.GetByFlightNumberAsync(flightNumber);
            if (flight == null)
                return null;

            var aircraft = await _aircraftRepository.GetByIdAsync(flight.AircraftId);
            return MapToResponseDto(flight, aircraft!);
        }

        public async Task<IEnumerable<FlightResponseDto>> GetAllFlightsAsync()
        {
            var flights = await _flightRepository.GetAllAsync();
            var result = new List<FlightResponseDto>();

            foreach (var flight in flights)
            {
                var aircraft = await _aircraftRepository.GetByIdAsync(flight.AircraftId);
                result.Add(MapToResponseDto(flight, aircraft!));
            }

            return result;
        }

        public async Task<IEnumerable<FlightResponseDto>> GetActiveFlightsAsync()
        {
            var flights = await _flightRepository.GetActiveFlightsAsync();
            return flights.Select(f => MapToResponseDto(f, f.Aircraft));
        }

        public async Task<IEnumerable<FlightResponseDto>> GetFlightsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var flights = await _flightRepository.GetFlightsByDateRangeAsync(startDate, endDate);
            return flights.Select(f => MapToResponseDto(f, f.Aircraft));
        }

        public async Task<IEnumerable<FlightResponseDto>> GetFlightsByAircraftAsync(int aircraftId)
        {
            var flights = await _flightRepository.GetFlightsByAircraftAsync(aircraftId);
            var aircraft = await _aircraftRepository.GetByIdAsync(aircraftId);

            return flights.Select(f => MapToResponseDto(f, aircraft!));
        }

        public async Task<IEnumerable<FlightResponseDto>> SearchFlightsAsync(
            string? flightNumber,
            string? departure,
            string? arrival)
        {
            var flights = await _flightRepository.SearchFlightsAsync(flightNumber, departure, arrival);
            return flights.Select(f => MapToResponseDto(f, f.Aircraft));
        }

        public async Task<FlightResponseDto> CreateFlightAsync(CreateFlightDto createDto)
        {
            // Validate aircraft exists
            var aircraft = await _aircraftRepository.GetByIdAsync(createDto.AircraftId);
            if (aircraft == null)
                throw new KeyNotFoundException("Uçak bulunamadı");

            if (!aircraft.IsActive)
                throw new InvalidOperationException("Uçak aktif değil");

            // Validate flight data
            await ValidateFlightAsync(createDto);

            // Check distance vs aircraft range
            if (createDto.DistanceKm > aircraft.MaxRangeKm)
                throw new InvalidOperationException(
                    $"Uçuş mesafesi ({createDto.DistanceKm} km), uçağın maksimum menzilini ({aircraft.MaxRangeKm} km) aşıyor");

            var flight = new Flight
            {
                FlightNumber = createDto.FlightNumber,
                AircraftId = createDto.AircraftId,
                DepartureCountry = createDto.DepartureCountry,
                DepartureCity = createDto.DepartureCity,
                DepartureAirport = createDto.DepartureAirport,
                DepartureAirportCode = createDto.DepartureAirportCode,
                ArrivalCountry = createDto.ArrivalCountry,
                ArrivalCity = createDto.ArrivalCity,
                ArrivalAirport = createDto.ArrivalAirport,
                ArrivalAirportCode = createDto.ArrivalAirportCode,
                DepartureTime = createDto.DepartureTime,
                ArrivalTime = createDto.ArrivalTime,
                DurationMinutes = createDto.DurationMinutes,
                DistanceKm = createDto.DistanceKm,
                CodeShareFlightNumber = createDto.CodeShareFlightNumber,
                CodeShareAirline = createDto.CodeShareAirline,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _flightRepository.AddAsync(flight);
            _logger.LogInformation("Flight created: {FlightNumber}", flight.FlightNumber);

            return MapToResponseDto(flight, aircraft);
        }

        public async Task<FlightResponseDto> UpdateFlightAsync(int flightId, UpdateFlightDto updateDto)
        {
            var flight = await _flightRepository.GetByIdAsync(flightId);
            if (flight == null)
                throw new KeyNotFoundException("Uçuş bulunamadı");

            var aircraft = await _aircraftRepository.GetByIdAsync(flight.AircraftId);

            if (!string.IsNullOrEmpty(updateDto.FlightNumber))
                flight.FlightNumber = updateDto.FlightNumber;

            if (updateDto.AircraftId.HasValue)
            {
                var newAircraft = await _aircraftRepository.GetByIdAsync(updateDto.AircraftId.Value);
                if (newAircraft == null)
                    throw new KeyNotFoundException("Yeni uçak bulunamadı");

                if (!newAircraft.IsActive)
                    throw new InvalidOperationException("Yeni uçak aktif değil");

                flight.AircraftId = updateDto.AircraftId.Value;
                aircraft = newAircraft;
            }

            if (!string.IsNullOrEmpty(updateDto.DepartureCountry))
                flight.DepartureCountry = updateDto.DepartureCountry;

            if (!string.IsNullOrEmpty(updateDto.DepartureCity))
                flight.DepartureCity = updateDto.DepartureCity;

            if (!string.IsNullOrEmpty(updateDto.DepartureAirport))
                flight.DepartureAirport = updateDto.DepartureAirport;

            if (!string.IsNullOrEmpty(updateDto.DepartureAirportCode))
                flight.DepartureAirportCode = updateDto.DepartureAirportCode;

            if (!string.IsNullOrEmpty(updateDto.ArrivalCountry))
                flight.ArrivalCountry = updateDto.ArrivalCountry;

            if (!string.IsNullOrEmpty(updateDto.ArrivalCity))
                flight.ArrivalCity = updateDto.ArrivalCity;

            if (!string.IsNullOrEmpty(updateDto.ArrivalAirport))
                flight.ArrivalAirport = updateDto.ArrivalAirport;

            if (!string.IsNullOrEmpty(updateDto.ArrivalAirportCode))
                flight.ArrivalAirportCode = updateDto.ArrivalAirportCode;

            if (updateDto.DepartureTime.HasValue)
                flight.DepartureTime = updateDto.DepartureTime.Value;

            if (updateDto.ArrivalTime.HasValue)
                flight.ArrivalTime = updateDto.ArrivalTime.Value;

            if (updateDto.DurationMinutes.HasValue)
                flight.DurationMinutes = updateDto.DurationMinutes.Value;

            if (updateDto.DistanceKm.HasValue)
            {
                if (updateDto.DistanceKm.Value > aircraft!.MaxRangeKm)
                    throw new InvalidOperationException(
                        $"Uçuş mesafesi ({updateDto.DistanceKm.Value} km), uçağın maksimum menzilini ({aircraft.MaxRangeKm} km) aşıyor");

                flight.DistanceKm = updateDto.DistanceKm.Value;
            }

            if (updateDto.CodeShareFlightNumber != null)
                flight.CodeShareFlightNumber = updateDto.CodeShareFlightNumber;

            if (updateDto.CodeShareAirline != null)
                flight.CodeShareAirline = updateDto.CodeShareAirline;

            if (updateDto.IsActive.HasValue)
                flight.IsActive = updateDto.IsActive.Value;

            // Validate times
            if (flight.ArrivalTime <= flight.DepartureTime)
                throw new InvalidOperationException("Varış zamanı, kalkış zamanından sonra olmalı");

            flight.UpdatedAt = DateTime.UtcNow;

            await _flightRepository.UpdateAsync(flight);
            _logger.LogInformation("Flight updated: {FlightId}", flightId);

            return MapToResponseDto(flight, aircraft!);
        }

        public async Task<bool> DeleteFlightAsync(int flightId)
        {
            var flight = await _flightRepository.GetByIdAsync(flightId);
            if (flight == null)
                throw new KeyNotFoundException("Uçuş bulunamadı");

            await _flightRepository.DeleteAsync(flight);
            _logger.LogInformation("Flight deleted: {FlightId}", flightId);

            return true;
        }

        public async Task<bool> ValidateFlightAsync(CreateFlightDto flightDto)
        {
            // Validate times
            if (flightDto.ArrivalTime <= flightDto.DepartureTime)
                throw new InvalidOperationException("Varış zamanı, kalkış zamanından sonra olmalı");

            // Validate departure is not in the past
            if (flightDto.DepartureTime < DateTime.UtcNow)
                throw new InvalidOperationException("Kalkış zamanı geçmişte olamaz");

            // Validate duration
            var actualDuration = (flightDto.ArrivalTime - flightDto.DepartureTime).TotalMinutes;
            if (Math.Abs(actualDuration - flightDto.DurationMinutes) > 30)
                throw new InvalidOperationException("Uçuş süresi, kalkış ve varış zamanları arasındaki farka uygun değil");

            // Validate same departure and arrival
            if (flightDto.DepartureAirportCode == flightDto.ArrivalAirportCode)
                throw new InvalidOperationException("Kalkış ve varış havaalanları aynı olamaz");

            return true;
        }

        private FlightResponseDto MapToResponseDto(Flight flight, Aircraft aircraft)
        {
            return new FlightResponseDto
            {
                FlightId = flight.FlightId,
                FlightNumber = flight.FlightNumber,
                AircraftId = flight.AircraftId,
                AircraftType = aircraft.AircraftType,
                DepartureCountry = flight.DepartureCountry,
                DepartureCity = flight.DepartureCity,
                DepartureAirport = flight.DepartureAirport,
                DepartureAirportCode = flight.DepartureAirportCode,
                ArrivalCountry = flight.ArrivalCountry,
                ArrivalCity = flight.ArrivalCity,
                ArrivalAirport = flight.ArrivalAirport,
                ArrivalAirportCode = flight.ArrivalAirportCode,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                DurationMinutes = flight.DurationMinutes,
                DistanceKm = flight.DistanceKm,
                CodeShareFlightNumber = flight.CodeShareFlightNumber,
                CodeShareAirline = flight.CodeShareAirline,
                CreatedAt = flight.CreatedAt,
                IsActive = flight.IsActive
            };
        }

        private FlightDetailResponseDto MapToDetailResponseDto(Flight flight)
        {
            var pilots = flight.FlightCrews.Where(fc => fc.IsActive).Select(fc => new FlightCrewMemberDto
            {
                PilotId = fc.Pilot.PilotId,
                FullName = fc.Pilot.User.FullName,
                Role = fc.Role,
                Seniority = fc.Pilot.Seniority.ToString(),
                LicenseNumber = fc.Pilot.LicenseNumber
            }).ToList();

            var cabinCrew = flight.FlightCabinCrews.Where(fcc => fcc.IsActive).Select(fcc => new FlightCabinCrewMemberDto
            {
                CabinCrewId = fcc.CabinCrew.CabinCrewId,
                FullName = fcc.CabinCrew.User.FullName,
                CrewType = fcc.CabinCrew.CrewType.ToString(),
                Seniority = fcc.CabinCrew.Seniority.ToString(),
                AssignedRecipe = fcc.AssignedRecipe
            }).ToList();

            var passengers = flight.Seats.Where(s => s.Passenger != null).Select(s => new FlightPassengerDto
            {
                PassengerId = s.Passenger!.PassengerId,
                FullName = s.Passenger.User.FullName,
                Age = CalculateAge(s.Passenger.User.DateOfBirth),
                Nationality = s.Passenger.User.Nationality,
                Gender = s.Passenger.User.Gender,
                SeatNumber = s.SeatNumber,
                SeatClass = s.SeatClass.ToString(),
                IsInfant = s.IsInfantSeat,
                ParentName = s.ParentPassenger?.User.FullName
            }).ToList();

            var occupiedSeats = flight.Seats.Count(s => s.IsOccupied);

            return new FlightDetailResponseDto
            {
                FlightId = flight.FlightId,
                FlightNumber = flight.FlightNumber,
                AircraftId = flight.AircraftId,
                AircraftType = flight.Aircraft.AircraftType,
                DepartureCountry = flight.DepartureCountry,
                DepartureCity = flight.DepartureCity,
                DepartureAirport = flight.DepartureAirport,
                DepartureAirportCode = flight.DepartureAirportCode,
                ArrivalCountry = flight.ArrivalCountry,
                ArrivalCity = flight.ArrivalCity,
                ArrivalAirport = flight.ArrivalAirport,
                ArrivalAirportCode = flight.ArrivalAirportCode,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                DurationMinutes = flight.DurationMinutes,
                DistanceKm = flight.DistanceKm,
                CodeShareFlightNumber = flight.CodeShareFlightNumber,
                CodeShareAirline = flight.CodeShareAirline,
                CreatedAt = flight.CreatedAt,
                IsActive = flight.IsActive,
                Pilots = pilots,
                CabinCrew = cabinCrew,
                Passengers = passengers,
                TotalSeats = flight.Aircraft.TotalSeats,
                OccupiedSeats = occupiedSeats
            };
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}