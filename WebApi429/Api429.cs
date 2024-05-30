using System.Text.Json.Serialization;

namespace WebApi429
{
    /// <summary>
    /// Represents rate-limiting state for mock API.
    /// </summary>
    public class Api429
    {
        /// <summary>
        /// Gets or sets the index of the API.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the count of requests made to the API.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the time when the API's rate limit will be reset.
        /// </summary>
        [JsonIgnore]
        public DateTime Reset429 { get; set; }

        /// <summary>
        /// Gets or sets the time of the last request made to the API.
        /// </summary>
        public DateTime LastRequest { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Api429"/> class.
        /// </summary>
        /// <param name="index">The index of the API.</param>
        public Api429(int index)
        {
            Index = index;
            Reset();
        }

        /// <summary>
        /// Adds a request to the API's rate limit.
        /// </summary>
        public void Add()
        {
            Count++;
            LastRequest = DateTime.UtcNow;
            Reset429 = DateTime.MinValue;
        }

        /// <summary>
        /// Resets the API's rate limit and the time of the last request.
        /// </summary>
        public void Reset()
        {
            Count = 0;
            LastRequest = DateTime.MinValue;
            Reset429 = DateTime.MinValue;            
        }

        /// <summary>
        /// Sets the HTTP 429 state.
        /// </summary>
        /// <param name="retryAfterSeconds"></param>
        public void Set429(int retryAfterSeconds)
        {
            Reset429 = DateTime.UtcNow.AddSeconds(retryAfterSeconds);
        }
    }
}
