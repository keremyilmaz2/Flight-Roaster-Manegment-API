using FlightRosterAPI.Models.DTOs.Passenger;
using FlightRosterAPI.Models.DTOs.Seat;
using FlightRosterAPI.Models.Enums;

namespace FlightRosterAPI.Services.IServices
{
    public interface IPassengerService
    {
        Task<PassengerResponseDto?> GetPassengerByIdAsync(int passengerId);
        Task<PassengerResponseDto?> GetPassengerByUserIdAsync(int userId);
        Task<IEnumerable<PassengerResponseDto>> GetAllPassengersAsync();
        Task<IEnumerable<PassengerResponseDto>> GetActivePassengersAsync();
        Task<IEnumerable<PassengerResponseDto>> GetPassengersByFlightAsync(int flightId);
        Task<PassengerResponseDto> CreatePassengerAsync(CreatePassengerDto createDto);
        Task<PassengerResponseDto> UpdatePassengerAsync(int passengerId, UpdatePassengerDto updateDto);
        Task<bool> DeletePassengerAsync(int passengerId);
    }

    public interface ISeatService
    {
        Task<SeatResponseDto?> GetSeatByIdAsync(int seatId);
        Task<SeatMapResponseDto?> GetSeatMapByFlightAsync(int flightId);
        Task<IEnumerable<SeatResponseDto>> GetAvailableSeatsByFlightAsync(int flightId);
        Task<IEnumerable<SeatResponseDto>> GetSeatsByClassAsync(int flightId, SeatClass seatClass);
        Task<SeatResponseDto> CreateSeatAsync(CreateSeatDto createDto);
        Task<SeatResponseDto> BookSeatAsync(int seatId, BookSeatDto bookDto);
        Task<SeatResponseDto> UpdateSeatAsync(int seatId, UpdateSeatDto updateDto);
        Task<bool> DeleteSeatAsync(int seatId);
        Task<bool> AutoAssignSeatsAsync(int flightId);
        Task<bool> GenerateSeatsForFlightAsync(int flightId);
    }
}