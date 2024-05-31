using System.Text.Json.Serialization;

namespace WebApi429
{
    /// <summary>
    /// Represents rate-limiting state for mock API.
    /// </summary>
    public class Api429Different : Api429
    {
        [JsonPropertyOrder(6)]
        public int RetryAfterSeconds { get; set; }

        [JsonPropertyOrder(5)]
        public int MaxRequests { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Api429"/> class.
        /// </summary>
        /// <param name="index">The index of the API.</param>
        public Api429Different(int index, int maxRequests, int retryAfterSeconds): base(index)
        {
            Reset();
            MaxRequests = maxRequests;
            RetryAfterSeconds = retryAfterSeconds;
        }
    }
}
