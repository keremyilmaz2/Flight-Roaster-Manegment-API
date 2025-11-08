using FlightRosterAPI.Models.DTOs.Flight;

namespace FlightRosterAPI.Services.IServices
{
    public interface IFlightService
    {
        Task<FlightResponseDto?> GetFlightByIdAsync(int flightId);
        Task<FlightDetailResponseDto?> GetFlightWithDetailsAsync(int flightId);
        Task<FlightResponseDto?> GetFlightByFlightNumberAsync(string flightNumber);
        Task<IEnumerable<FlightResponseDto>> GetAllFlightsAsync();
        Task<IEnumerable<FlightResponseDto>> GetActiveFlightsAsync();
        Task<IEnumerable<FlightResponseDto>> GetFlightsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<FlightResponseDto>> GetFlightsByAircraftAsync(int aircraftId);
        Task<IEnumerable<FlightResponseDto>> SearchFlightsAsync(string? flightNumber, string? departure, string? arrival);
        Task<FlightResponseDto> CreateFlightAsync(CreateFlightDto createDto);
        Task<FlightResponseDto> UpdateFlightAsync(int flightId, UpdateFlightDto updateDto);
        Task<bool> DeleteFlightAsync(int flightId);
        Task<bool> ValidateFlightAsync(CreateFlightDto flightDto);
    }
}