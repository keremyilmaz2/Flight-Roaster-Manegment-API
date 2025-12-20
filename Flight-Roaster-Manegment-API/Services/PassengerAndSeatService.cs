using FlightRosterAPI.Models.DTOs.Passenger;
using FlightRosterAPI.Models.DTOs.Seat;
using FlightRosterAPI.Models.Entities;
using FlightRosterAPI.Models.Enums;
using FlightRosterAPI.Repositories.Interfaces;
using FlightRosterAPI.Services.IServices;

namespace FlightRosterAPI.Services
{
    public class PassengerService : IPassengerService
    {
        private readonly IPassengerRepository _passengerRepository;
        private readonly ILogger<PassengerService> _logger;

        public PassengerService(
            IPassengerRepository passengerRepository,
            ILogger<PassengerService> logger)
        {
            _passengerRepository = passengerRepository;
            _logger = logger;
        }

        public async Task<PassengerResponseDto?> GetPassengerByIdAsync(int passengerId)
        {
            var passenger = await _passengerRepository.GetPassengerWithUserAsync(passengerId);
            return passenger != null ? MapToResponseDto(passenger) : null;
        }

        public async Task<PassengerResponseDto?> GetPassengerByUserIdAsync(int userId)
        {
            var passenger = await _passengerRepository.GetByUserIdAsync(userId);
            return passenger != null ? MapToResponseDto(passenger) : null;
        }

        public async Task<IEnumerable<PassengerResponseDto>> GetAllPassengersAsync()
        {
            var passengers = await _passengerRepository.GetAllAsync();
            var result = new List<PassengerResponseDto>();

            foreach (var passenger in passengers)
            {
                var passengerWithUser = await _passengerRepository.GetPassengerWithUserAsync(passenger.PassengerId);
                if (passengerWithUser != null)
                    result.Add(MapToResponseDto(passengerWithUser));
            }

            return result;
        }

        public async Task<IEnumerable<PassengerResponseDto>> GetActivePassengersAsync()
        {
            var passengers = await _passengerRepository.GetActivePassengersAsync();
            return passengers.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<PassengerResponseDto>> GetPassengersByFlightAsync(int flightId)
        {
            var passengers = await _passengerRepository.GetPassengersByFlightAsync(flightId);
            return passengers.Select(MapToResponseDto);
        }

        public async Task<PassengerResponseDto> CreatePassengerAsync(CreatePassengerDto createDto)
        {
            // Validate passport number uniqueness if provided
            if (!string.IsNullOrEmpty(createDto.PassportNumber))
            {
                var isUnique = await _passengerRepository.IsPassportNumberUniqueAsync(createDto.PassportNumber);
                if (!isUnique)
                    throw new InvalidOperationException("Bu pasaport numarası zaten kullanılıyor");
            }

            var passenger = new Passenger
            {
                UserId = createDto.UserId,
                PassportNumber = createDto.PassportNumber,
                NationalIdNumber = createDto.NationalIdNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _passengerRepository.AddAsync(passenger);
            _logger.LogInformation("Passenger created: {PassengerId}", passenger.PassengerId);

            var createdPassenger = await _passengerRepository.GetPassengerWithUserAsync(passenger.PassengerId);
            return MapToResponseDto(createdPassenger!);
        }

        public async Task<PassengerResponseDto> UpdatePassengerAsync(int passengerId, UpdatePassengerDto updateDto)
        {
            var passenger = await _passengerRepository.GetPassengerWithUserAsync(passengerId);
            if (passenger == null)
                throw new KeyNotFoundException("Yolcu bulunamadı");

            // Check passport number uniqueness if being updated
            if (!string.IsNullOrEmpty(updateDto.PassportNumber) &&
                updateDto.PassportNumber != passenger.PassportNumber)
            {
                var isUnique = await _passengerRepository.IsPassportNumberUniqueAsync(
                    updateDto.PassportNumber, passengerId);
                if (!isUnique)
                    throw new InvalidOperationException("Bu pasaport numarası zaten kullanılıyor");

                passenger.PassportNumber = updateDto.PassportNumber;
            }

            if (updateDto.NationalIdNumber != null)
                passenger.NationalIdNumber = updateDto.NationalIdNumber;

            if (updateDto.IsActive.HasValue)
                passenger.IsActive = updateDto.IsActive.Value;

            passenger.UpdatedAt = DateTime.UtcNow;

            await _passengerRepository.UpdateAsync(passenger);
            _logger.LogInformation("Passenger updated: {PassengerId}", passengerId);

            var updatedPassenger = await _passengerRepository.GetPassengerWithUserAsync(passengerId);
            return MapToResponseDto(updatedPassenger!);
        }

        public async Task<bool> DeletePassengerAsync(int passengerId)
        {
            var passenger = await _passengerRepository.GetByIdAsync(passengerId);
            if (passenger == null)
                throw new KeyNotFoundException("Yolcu bulunamadı");

            await _passengerRepository.DeleteAsync(passenger);
            _logger.LogInformation("Passenger deleted: {PassengerId}", passengerId);

            return true;
        }

        private PassengerResponseDto MapToResponseDto(Passenger passenger)
        {
            return new PassengerResponseDto
            {
                PassengerId = passenger.PassengerId,
                UserId = passenger.UserId,
                FirstName = passenger.User.FirstName,
                LastName = passenger.User.LastName,
                Email = passenger.User.Email!,
                DateOfBirth = passenger.User.DateOfBirth,
                Age = CalculateAge(passenger.User.DateOfBirth),
                Nationality = passenger.User.Nationality,
                Gender = passenger.User.Gender,
                PassportNumber = passenger.PassportNumber,
                NationalIdNumber = passenger.NationalIdNumber,
                CreatedAt = passenger.CreatedAt,
                IsActive = passenger.IsActive
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

    public class SeatService : ISeatService
    {
        private readonly ISeatRepository _seatRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly ILogger<SeatService> _logger;

        public SeatService(
            ISeatRepository seatRepository,
            IFlightRepository flightRepository,
            IPassengerRepository passengerRepository,
            ILogger<SeatService> logger)
        {
            _seatRepository = seatRepository;
            _flightRepository = flightRepository;
            _passengerRepository = passengerRepository;
            _logger = logger;
        }

        public async Task<SeatResponseDto?> GetSeatByIdAsync(int seatId)
        {
            var seat = await _seatRepository.GetSeatWithDetailsAsync(seatId);
            return seat != null ? MapToResponseDto(seat) : null;
        }

        public async Task<SeatMapResponseDto?> GetSeatMapByFlightAsync(int flightId)
        {
            var flight = await _flightRepository.GetFlightWithDetailsAsync(flightId);
            if (flight == null)
                return null;

            var seats = await _seatRepository.GetSeatsByFlightAsync(flightId);
            var seatDtos = seats.Select(MapToResponseDto).ToList();

            return new SeatMapResponseDto
            {
                FlightId = flight.FlightId,
                FlightNumber = flight.FlightNumber,
                AircraftType = flight.Aircraft.AircraftType,
                TotalSeats = flight.Aircraft.TotalSeats,
                AvailableSeats = seats.Count(s => !s.IsOccupied),
                OccupiedSeats = seats.Count(s => s.IsOccupied),
                Seats = seatDtos
            };
        }

        public async Task<IEnumerable<SeatResponseDto>> GetAvailableSeatsByFlightAsync(int flightId)
        {
            var seats = await _seatRepository.GetAvailableSeatsByFlightAsync(flightId);
            return seats.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<SeatResponseDto>> GetSeatsByPassengerAsync(int passengerId)
        {
            var seats = await _seatRepository.GetSeatsByPassengerAsync(passengerId);
            return seats.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<SeatResponseDto>> GetSeatsByClassAsync(int flightId, SeatClass seatClass)
        {
            var seats = await _seatRepository.GetSeatsByClassAsync(flightId, seatClass);
            return seats.Select(MapToResponseDto);
        }

        public async Task<SeatResponseDto> CreateSeatAsync(CreateSeatDto createDto)
        {
            // Validate flight exists
            var flight = await _flightRepository.GetByIdAsync(createDto.FlightId);
            if (flight == null)
                throw new KeyNotFoundException("Uçuş bulunamadı");

            // Validate seat number uniqueness
            var isUnique = await _seatRepository.IsSeatNumberUniqueAsync(
                createDto.FlightId, createDto.SeatNumber);
            if (!isUnique)
                throw new InvalidOperationException("Bu koltuk numarası bu uçuşta zaten mevcut");

            var seat = new Seat
            {
                FlightId = createDto.FlightId,
                SeatNumber = createDto.SeatNumber,
                SeatClass = createDto.SeatClass,
                IsInfantSeat = false,
                IsOccupied = false,
                CreatedAt = DateTime.UtcNow
            };

            await _seatRepository.AddAsync(seat);
            _logger.LogInformation("Seat created: {SeatNumber} for flight {FlightId}",
                seat.SeatNumber, seat.FlightId);

            var createdSeat = await _seatRepository.GetSeatWithDetailsAsync(seat.SeatId);
            return MapToResponseDto(createdSeat!);
        }

        public async Task<SeatResponseDto> BookSeatAsync(int seatId, BookSeatDto bookDto)
        {
            var seat = await _seatRepository.GetSeatWithDetailsAsync(seatId);
            if (seat == null)
                throw new KeyNotFoundException("Koltuk bulunamadı");

            if (seat.IsOccupied)
                throw new InvalidOperationException("Koltuk zaten dolu");

            // Validate passenger exists
            var passenger = await _passengerRepository.GetByIdAsync(bookDto.PassengerId);
            if (passenger == null)
                throw new KeyNotFoundException("Yolcu bulunamadı");

            // Handle infant seat booking
            if (bookDto.IsInfantSeat)
            {
                if (!bookDto.ParentPassengerId.HasValue)
                    throw new InvalidOperationException("Bebek koltuğu için ebeveyn ID'si gerekli");

                var parentPassenger = await _passengerRepository.GetByIdAsync(bookDto.ParentPassengerId.Value);
                if (parentPassenger == null)
                    throw new KeyNotFoundException("Ebeveyn yolcu bulunamadı");

                seat.IsInfantSeat = true;
                seat.ParentPassengerId = bookDto.ParentPassengerId.Value;
            }

            seat.PassengerId = bookDto.PassengerId;
            seat.IsOccupied = true;
            seat.BookedAt = DateTime.UtcNow;
            seat.UpdatedAt = DateTime.UtcNow;

            await _seatRepository.UpdateAsync(seat);
            _logger.LogInformation("Seat {SeatId} booked for passenger {PassengerId}",
                seatId, bookDto.PassengerId);

            var updatedSeat = await _seatRepository.GetSeatWithDetailsAsync(seatId);
            return MapToResponseDto(updatedSeat!);
        }

        public async Task<SeatResponseDto> UpdateSeatAsync(int seatId, UpdateSeatDto updateDto)
        {
            var seat = await _seatRepository.GetSeatWithDetailsAsync(seatId);
            if (seat == null)
                throw new KeyNotFoundException("Koltuk bulunamadı");

            if (!string.IsNullOrEmpty(updateDto.SeatNumber) && updateDto.SeatNumber != seat.SeatNumber)
            {
                var isUnique = await _seatRepository.IsSeatNumberUniqueAsync(
                    seat.FlightId, updateDto.SeatNumber, seatId);
                if (!isUnique)
                    throw new InvalidOperationException("Bu koltuk numarası bu uçuşta zaten mevcut");

                seat.SeatNumber = updateDto.SeatNumber;
            }

            if (updateDto.SeatClass.HasValue)
                seat.SeatClass = updateDto.SeatClass.Value;

            if (updateDto.PassengerId.HasValue)
                seat.PassengerId = updateDto.PassengerId.Value;

            if (updateDto.IsInfantSeat.HasValue)
                seat.IsInfantSeat = updateDto.IsInfantSeat.Value;

            if (updateDto.ParentPassengerId.HasValue)
                seat.ParentPassengerId = updateDto.ParentPassengerId.Value;

            if (updateDto.IsOccupied.HasValue)
                seat.IsOccupied = updateDto.IsOccupied.Value;

            seat.UpdatedAt = DateTime.UtcNow;

            await _seatRepository.UpdateAsync(seat);
            _logger.LogInformation("Seat updated: {SeatId}", seatId);

            var updatedSeat = await _seatRepository.GetSeatWithDetailsAsync(seatId);
            return MapToResponseDto(updatedSeat!);
        }

        public async Task<bool> DeleteSeatAsync(int seatId)
        {
            var seat = await _seatRepository.GetByIdAsync(seatId);
            if (seat == null)
                throw new KeyNotFoundException("Koltuk bulunamadı");

            if (seat.IsOccupied)
                throw new InvalidOperationException("Dolu koltuk silinemez");

            await _seatRepository.DeleteAsync(seat);
            _logger.LogInformation("Seat deleted: {SeatId}", seatId);

            return true;
        }

        public async Task<bool> AutoAssignSeatsAsync(int flightId)
        {
            var flight = await _flightRepository.GetFlightWithDetailsAsync(flightId);
            if (flight == null)
                throw new KeyNotFoundException("Uçuş bulunamadı");

            var passengers = flight.Seats
                .Where(s => s.Passenger != null && !s.IsOccupied)
                .Select(s => s.Passenger!)
                .Distinct()
                .ToList();

            var availableSeats = (await _seatRepository.GetAvailableSeatsByFlightAsync(flightId)).ToList();

            if (passengers.Count > availableSeats.Count)
                throw new InvalidOperationException("Yeterli boş koltuk yok");

            // Assign seats to passengers (simple algorithm - could be improved)
            for (int i = 0; i < passengers.Count; i++)
            {
                var seat = availableSeats[i];
                var passenger = passengers[i];

                var seatEntity = await _seatRepository.GetByIdAsync(seat.SeatId);
                if (seatEntity != null)
                {
                    seatEntity.PassengerId = passenger.PassengerId;
                    seatEntity.IsOccupied = true;
                    seatEntity.BookedAt = DateTime.UtcNow;
                    await _seatRepository.UpdateAsync(seatEntity);
                }
            }

            _logger.LogInformation("Auto-assigned {Count} seats for flight {FlightId}",
                passengers.Count, flightId);

            return true;
        }

        public async Task<bool> GenerateSeatsForFlightAsync(int flightId)
        {
            var flight = await _flightRepository.GetFlightWithDetailsAsync(flightId);
            if (flight == null)
                throw new KeyNotFoundException("Uçuş bulunamadı");

            var existingSeats = await _seatRepository.GetSeatsByFlightAsync(flightId);
            if (existingSeats.Any())
                throw new InvalidOperationException("Bu uçuş için koltuklar zaten oluşturulmuş");

            var seats = new List<Seat>();
            int seatNumber = 1;

            // Generate business class seats
            for (int i = 0; i < flight.Aircraft.BusinessClassSeats; i++)
            {
                var row = (i / 4) + 1;
                var col = (char)('A' + (i % 4));

                seats.Add(new Seat
                {
                    FlightId = flightId,
                    SeatNumber = $"{row}{col}",
                    SeatClass = SeatClass.Business,
                    IsOccupied = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Generate economy class seats
            int economyStartRow = (flight.Aircraft.BusinessClassSeats / 4) + 2;
            for (int i = 0; i < flight.Aircraft.EconomyClassSeats; i++)
            {
                var row = economyStartRow + (i / 6);
                var col = (char)('A' + (i % 6));

                seats.Add(new Seat
                {
                    FlightId = flightId,
                    SeatNumber = $"{row}{col}",
                    SeatClass = SeatClass.Economy,
                    IsOccupied = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _seatRepository.AddRangeAsync(seats);
            _logger.LogInformation("Generated {Count} seats for flight {FlightId}",
                seats.Count, flightId);

            return true;
        }

        // Services/SeatService.cs içinde

        private SeatResponseDto MapToResponseDto(Seat seat)
        {
            return new SeatResponseDto
            {
                SeatId = seat.SeatId,
                FlightId = seat.FlightId,
                FlightNumber = seat.Flight?.FlightNumber ?? string.Empty,

                // ✅ Detaylı Flight Bilgileri
                FlightInfo = seat.Flight != null ? MapToFlightInfoDto(seat.Flight) : null,

                PassengerId = seat.PassengerId,
                PassengerName = seat.Passenger?.User.FullName,
                SeatNumber = seat.SeatNumber,
                SeatClass = seat.SeatClass,
                IsInfantSeat = seat.IsInfantSeat,
                ParentPassengerId = seat.ParentPassengerId,
                ParentPassengerName = seat.ParentPassenger?.User.FullName,
                IsOccupied = seat.IsOccupied,
                BookedAt = seat.BookedAt,
                CreatedAt = seat.CreatedAt
            };
        }

        // Services/SeatService.cs içinde

        private FlightInfoDto MapToFlightInfoDto(Flight flight)
        {
            var now = DateTime.UtcNow;

            return new FlightInfoDto
            {
                FlightId = flight.FlightId,
                FlightNumber = flight.FlightNumber,

                // Kalkış
                DepartureCountry = flight.DepartureCountry,
                DepartureCity = flight.DepartureCity,
                DepartureAirport = flight.DepartureAirport,
                DepartureAirportCode = flight.DepartureAirportCode,
                DepartureTime = flight.DepartureTime,

                // Varış
                ArrivalCountry = flight.ArrivalCountry,
                ArrivalCity = flight.ArrivalCity,
                ArrivalAirport = flight.ArrivalAirport,
                ArrivalAirportCode = flight.ArrivalAirportCode,
                ArrivalTime = flight.ArrivalTime,

                // Detaylar
                DurationMinutes = flight.DurationMinutes,
                DurationText = FormatDuration(flight.DurationMinutes),
                DistanceKm = flight.DistanceKm,

                // Aircraft
                AircraftType = flight.Aircraft?.AircraftType,
                AircraftRegistration = flight.Aircraft?.RegistrationNumber,

                // Code Share
                CodeShareFlightNumber = flight.CodeShareFlightNumber,
                CodeShareAirline = flight.CodeShareAirline,

                // Computed
                RouteText = $"{flight.DepartureAirportCode} → {flight.ArrivalAirportCode}",
                IsDeparted = flight.DepartureTime <= now,
                IsCompleted = flight.ArrivalTime <= now
            };
        }

        // Duration Formatter
        private string FormatDuration(int durationMinutes)
        {
            var hours = durationMinutes / 60;
            var minutes = durationMinutes % 60;

            if (hours == 0)
                return $"{minutes}dk";

            if (minutes == 0)
                return $"{hours}s";

            return $"{hours}s {minutes}dk";
        }
    }
}