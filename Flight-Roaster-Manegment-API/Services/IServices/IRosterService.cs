using FlightRosterAPI.Models.DTOs.Roster;

namespace FlightRosterAPI.Services.IServices
{
    public interface IRosterService
    {
        // Roster Retrieval
        Task<FlightRosterResponseDto?> GetFlightRosterAsync(int flightId);
        Task<FlightRosterResponseDto?> GetFlightRosterByFlightNumberAsync(string flightNumber);

        // Manual Assignment
        Task<FlightCrewResponseDto> AssignPilotToFlightAsync(AssignFlightCrewDto assignDto);
        Task<FlightCabinCrewResponseDto> AssignCabinCrewToFlightAsync(AssignCabinCrewDto assignDto);
        Task<bool> RemovePilotFromFlightAsync(int flightId, int pilotId);
        Task<bool> RemoveCabinCrewFromFlightAsync(int flightId, int cabinCrewId);

        // Auto Assignment
        Task<FlightRosterResponseDto> AutoAssignCrewAsync(AutoAssignCrewDto autoAssignDto);
        Task<bool> ValidateFlightCrewAsync(int flightId);

        // Export
        Task<string> ExportRosterToJsonAsync(int flightId);
        Task<byte[]> ExportRosterToPdfAsync(int flightId);
    }
}