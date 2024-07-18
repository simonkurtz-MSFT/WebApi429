using System.Text;

namespace WebApi429
{
    public class Program
    {
        /// <summary>
        /// Generates API endpoints that return 429 status codes when the maximum number of requests is exceeded. The 429 logic is simplified through a counter and irrespective of requests over time.
        /// If the counter is at the maximum, the endpoint will return a 429 status code and reset the counter after a specified time.
        /// </summary>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();
            var parameters = builder.Configuration.GetSection("Parameters").Get<Parameters>() ?? throw new ArgumentNullException("Parameters", "Parameters are required."); // Assumes that if the Parameters section exist, so do the individual properties. No need to go overboard here.

            #region Sinmilar Endpoints
            var Tracker = new List<Api429>(parameters.MaxEndpoints);

            for (int i = 0; i < parameters.MaxEndpoints; i++)
            {
                Tracker.Add(new Api429(i));
            }
                        
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
                {   
                    var tracker = Tracker[i];

                    // Ensure that the existing endpoint can accept requests.
                    if (tracker.Reset429 > DateTime.UtcNow)
                    {
                        return Http429(context, (int)Math.Ceiling((tracker.Reset429 - DateTime.UtcNow).TotalSeconds));
                    }
                    else
                    {
                        // If the last request was more than the value in RetryAfterSeconds ago, reset the counter. This ensures a fresh start for the counter after a period of inactivity.
                        if (tracker.LastRequest.AddSeconds(parameters.RetryAfterSeconds) < DateTime.UtcNow)
                        {
                            tracker.Reset();
                        }

                        // Increment the counter for the successful request.
                        if (tracker.Count < parameters.MaxRequests)
                        {
                            tracker.Add();

                            return Results.Ok(tracker);
                        }
                        else // If the counter is at the maximum, return a 429.
                        {
                            tracker.Set429(parameters.RetryAfterSeconds);
                            return Http429(context, parameters.RetryAfterSeconds);
                        }
                    }
                }
            });
            #endregion

            #region Different Endpoints
            var TrackerDifferent = new List<Api429Different>(parameters.MaxEndpoints);
            var random = new Random();

            for (int i = 0; i < parameters.MaxEndpoints; i++)
            {
                // maxRequests can be between 5 and 50, retryAfterSeconds is between 2 and 10.
                TrackerDifferent.Add(new Api429Different(i, random.Next(5, 51), random.Next(2, 11)));
            }

            app.MapGet("/api2/{index}", (HttpContext context, string index) =>
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
                {
                    var tracker = TrackerDifferent[i];

                    // Ensure that the existing endpoint can accept requests.
                    if (tracker.Reset429 > DateTime.UtcNow)
                    {
                        return Http429(context, (int)Math.Ceiling((tracker.Reset429 - DateTime.UtcNow).TotalSeconds));
                    }
                    else
                    {
                        // If the last request was more than the value in RetryAfterSeconds ago, reset the counter. This ensures a fresh start for the counter after a period of inactivity.
                        if (tracker.LastRequest.AddSeconds(tracker.RetryAfterSeconds) < DateTime.UtcNow)
                        {
                            tracker.Reset();
                        }

                        // Increment the counter for the successful request.
                        if (tracker.Count < tracker.MaxRequests)
                        {
                            tracker.Add();

                            return Results.Ok(tracker);
                        }
                        else // If the counter is at the maximum, return a 429.
                        {
                            tracker.Set429(tracker.RetryAfterSeconds);
                            return Http429(context, tracker.RetryAfterSeconds);
                        }
                    }
                }
            });
            #endregion

            #region 500 Error
            app.MapGet("/api-500", (HttpContext context) =>
            {
                return Results.StatusCode(500);
            });
            #endregion

            #region Dropped Connection
            app.MapGet("/drop", (HttpContext context) =>
            {
                context.Abort();
            });
            #endregion

            #region Start page
            app.MapGet("/", (HttpContext context) =>
            {
                var response = new StringBuilder();
                response.Append("<html><head><title>WebApi429</title></head><body>");
                response.Append("<h2>WebApi429 Parameters & Endpoints</h2>");
                
                // Same Endpoints
                response.Append("<h3>Same Endpoints</h3>");
                response.Append($"<p>A total of <b>{parameters.MaxRequests}</b> requests can be issued against each of the <b>{parameters.MaxEndpoints}</b> endpoints. These endpoints all share the same max requests and retry-after value. Beyond that an HTTP 429 will be returned with a <code>Retry-After</code> header value of <b>{parameters.RetryAfterSeconds}</b> seconds.</p>");
                response.Append("<ul>");
                for (int i = 0; i < parameters.MaxEndpoints; i++)
                {
                    response.Append($"<li><a href=\"/api/{i}\" target=\"_blank\">/api/{i}</a></li>");
                }
                response.Append("</ul>");

                // Different Endpoints
                response.Append("<h3>Different Endpoints</h3>");
                response.Append($"<p>A variable number of requests can be issued against each of the <b>{parameters.MaxEndpoints}</b> endpoints. These endpoints have different max requests and retry-after values. Beyond that an HTTP 429 will be returned with a <code>Retry-After</code> header value in seconds.</p>");
                response.Append("<ul>");
                for (int i = 0; i < parameters.MaxEndpoints; i++)
                {
                    response.Append($"<li><a href=\"/api2/{i}\" target=\"_blank\">/api2/{i}</a></li>");
                }
                response.Append("</ul>");

                // 500 Error
                response.Append("<h3>500 Endpoint</h3>");
                response.Append("<ul>");
                response.Append($"<li><a href=\"/api-500\" target=\"_blank\">/api-500</a></li>");
                response.Append("</ul>");

                // Dropped Connection                
                response.Append("<h3>Dropped Connection Endpoint</h3>");
                response.Append("<ul>");
                response.Append($"<li><a href=\"/drop\" target=\"_blank\">/drop</a></li>");
                response.Append("</ul>");

                response.Append("</body></html>");

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

        private static IResult Http429(HttpContext context, int retryAfterSeconds)
        {
            context.Response.Headers.Append("Retry-After", retryAfterSeconds.ToString());
            return Results.StatusCode(429);
        }
    }
}
