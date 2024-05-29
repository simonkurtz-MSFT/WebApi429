namespace WebApi429
{
    /// <summary>
    /// Represents the parameters for the API.
    /// </summary>
    public class Parameters
    {
        /// <summary>
        /// Gets or sets the maximum number of endpoints.
        /// </summary>
        public int MaxEndpoints { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of requests.
        /// </summary>
        public int MaxRequests { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds to wait before retrying after a 429 status.
        /// </summary>
        public int RetryAfterSeconds { get; set; }
    }
}
