using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SmartLogistics.API.Middleware
{
    // Middleware to log every incoming Request and its corresponding outgoing Response
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Start a timer to measure the request execution time
            var timer = Stopwatch.StartNew();
            var traceId = context.TraceIdentifier;

            // Log incoming request details
            _logger.LogInformation("Incoming Request: {Method} {Path} [TraceId: {TraceId}]",
                context.Request.Method,
                context.Request.Path,
                traceId);

            // Pass the request to the next middleware in the pipeline
            await _next(context);

            timer.Stop();
            var elapsedMs = timer.ElapsedMilliseconds;
            var statusCode = context.Response.StatusCode;

            // Determine the log level based on the response status code
            if (statusCode >= 500)
            {
                _logger.LogError("Request Error: {Method} {Path} responded {StatusCode} in {Elapsed}ms",
                    context.Request.Method, context.Request.Path, statusCode, elapsedMs);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning("Request Warning: {Method} {Path} responded {StatusCode} in {Elapsed}ms",
                    context.Request.Method, context.Request.Path, statusCode, elapsedMs);
            }
            else
            {
                _logger.LogInformation("Request Success: {Method} {Path} responded {StatusCode} in {Elapsed}ms",
                    context.Request.Method, context.Request.Path, statusCode, elapsedMs);
            }
        }
    }
}