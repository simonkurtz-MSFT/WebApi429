using System.Text;

namespace WebApi429
{
    public class Program
    {
        public class Api429
        {
            public int Count { get; set; } = 0;
            public DateTime Reset429 { get; set; } = DateTime.UtcNow;
            public DateTime LastRequest { get; set; } = DateTime.UtcNow;
        }

        /// <summary>
        /// Generates API endpoints that return 429 status codes when the maximum number of requests is exceeded. The 429 logic is simplified through a counter and irrespective of requests over time.
        /// If the counter is at the maximum, the endpoint will return a 429 status code and reset the counter after a specified time.
        /// </summary>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();
            var parameters = builder.Configuration.GetSection("Parameters").Get<Parameters>() ?? throw new ArgumentNullException("Parameters", "Parameters are required."); // Assumes that if the Parameters section exist, so do the individual properties. No need to go overboard here.
            var Tracker = new List<Api429>(parameters.MaxEndpoints);

            for (int i = 0; i < parameters.MaxEndpoints; i++)
            {
                Tracker.Add(new Api429());
            }

            #region Endpoints
            app.MapGet("/api/{index}", (HttpContext context, string index) =>
            {
                if (!Int32.TryParse(index, out int i))
                {
                    return Results.NotFound();
                }

                // Ensure that the requested endpoint exists.
                if (i < 0 || i >= parameters.MaxEndpoints)
                {
                    return Results.NotFound();
                }
                else
                {   // Ensure that the existing endpoint can accept requests.
                    if (Tracker[i].Reset429 > DateTime.UtcNow)
                    {
                        context.Response.Headers.Append("Retry-After", Math.Ceiling((Tracker[i].Reset429 - DateTime.UtcNow).TotalSeconds).ToString());
                        return Results.StatusCode(429);
                    }
                    else
                    {
                        // If the last request was more than a minute ago, reset the counter. This ensures a fresh start for the counter after a period of inactivity.
                        if (Tracker[i].LastRequest.AddSeconds(parameters.ResetCounterAfterSeconds) < DateTime.UtcNow)
                        {
                            Tracker[i].Count = 0;
                        }

                        // Increment the counter for the successful request.
                        if (Tracker[i].Count < parameters.MaxRequests)
                        {
                            Tracker[i].Count++;
                            Tracker[i].LastRequest = DateTime.UtcNow;
                            return Results.Ok(i.ToString());
                        }
                        else // If the counter is at the maximum, return a 429.
                        {
                            Tracker[i].Count = 0;
                            Tracker[i].Reset429 = DateTime.UtcNow.AddSeconds(parameters.RetryAfterSeconds);

                            context.Response.Headers.Append("Retry-After", parameters.RetryAfterSeconds.ToString());
                            return Results.StatusCode(429);
                        }
                    }
                }
            });
            #endregion

            #region Start page
            app.MapGet("/", (HttpContext context) =>
            {
                var response = new StringBuilder();
                response.Append("<html><head><title>WebApi429</title></head><body><h2>WebApi429 Parameters & Endpoints</h2><p>");
                response.Append($"A total of <b>{parameters.MaxRequests}</b> requests can be issued against each of the <b>{parameters.MaxEndpoints}</b> endpoints over a time interval of <b>{parameters.ResetCounterAfterSeconds}</b> seconds. ");
                response.Append($"Beyond that an HTTP 429 will be returned with a <code>Retry-After</code> header value of <b>{parameters.RetryAfterSeconds}</b> seconds.<ul>");
                for (int i = 0; i < parameters.MaxEndpoints; i++)
                {
                    response.Append($"<li><a href=\"/api/{i}\" target=\"_blank\">/api/{i}</a></li>");
                }
                response.Append("</ul></body></html>");

                return Results.Content(response.ToString(), "text/html");
            });
            #endregion


            app.MapFallback(async (context) =>
            {
                context.Response.Redirect("/");
                await Task.CompletedTask;
            });

            app.Run();
        }
    }
}
