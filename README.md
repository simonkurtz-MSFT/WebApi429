# WebApi429

This is a simple Web API that returns mock HTTP 429 responses over a set of configurable endpoints.

The integer parameters that can be set are:

- MaxEndpoints
- MaxRequests
- RetryAfterSeconds

The Web API configures `MaxEndpoints` endpoints, starting at 0. For example, if `MaxEndpoints` is set to 3, the endpoints will be: `/api/0`, `/api/1`, `/api/2`.

`MaxRequests` is the maximum number of requests that can be made to an endpoint before it returns an HTTP 429 response. The counter is reset after `RetryAfterSeconds` seconds.

When an HTTP 429 response is returned, the `Retry-After` header is set to `RetryAfterSeconds`.