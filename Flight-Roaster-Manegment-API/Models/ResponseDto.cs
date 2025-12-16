namespace FlightRosterAPI.Models
{
    /// <summary>
    /// Standard API response wrapper for all endpoints
    /// </summary>
    public class ResponseDto
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// Message describing the result (success or error message)
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The actual data returned by the API (can be any type)
        /// </summary>
        public object? Result { get; set; }

        /// <summary>
        /// Additional metadata or error details (optional)
        /// </summary>
        public object? Metadata { get; set; }
    }
}