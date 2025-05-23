# WebApi429

This is a simple Web API that returns mock HTTP 429 responses over a set of configurable endpoints.

The integer parameters that can be set are:

- MaxEndpoints
- MaxRequests
- RetryAfterSeconds

The Web API configures `MaxEndpoints` endpoints, starting at 0. For example, if `MaxEndpoints` is set to 3, the endpoints will be: `/api/0`, `/api/1`, `/api/2`.

`MaxRequests` is the maximum number of requests that can be made to an endpoint before it returns an HTTP 429 response. The counter is reset after `RetryAfterSeconds` seconds.

When an HTTP 429 response is returned, the `Retry-After` header is set to `RetryAfterSeconds`.

The container image is hosted on [Docker Hub](https://hub.docker.com/r/simonkurtzmsft/webapi429).

## Developing

### Prerequisities

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- Switch to the directory into which you cloned the repo.

### Building & Running Locally

#### Build the Project

- Execute `dotnet build .\WebApi429\WebApi429.csproj` to verify the build works.

#### HTTP-only

- Execute `dotnet run --project .\WebApi429\WebApi429.csproj` to run. the Kestrel server with http only.
- Issue a curl to get a response: `curl -v http://localhost:20080/api/0`

#### HTTPS/HTTP

- Ensure that you trust the dev certificate: `dotnet dev-certs https --trust`
- Execute `dotnet run --project .\WebApi429\WebApi429.csproj --launch-profile https` to run the Kestrel server with http and https
- Issue a curl to get a response: `curl -v http://localhost:20080/api/0`
- Issue a curl to get a secure response: `curl -v https://localhost:20443/api/0`

### Building & Running as a Local Container

#### Build the Container

- Start the Docker daemon.
- Execute `docker build -t webapi429:1 -f .\WebApi429\Dockerfile .\WebApi429`

#### HTTP

The container runs under http because the container host where it may run conventionally terminates the secure connection.

- Execute `docker run -p 21080:8080 -t webapi429:1` to run. Note the different port to ensure we don't cross streams.
- Issue a curl to get a response: `curl -v http://localhost:21080/api/0`