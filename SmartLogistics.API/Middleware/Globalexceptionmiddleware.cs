using global::SmartLogistics.Application.Common.Exceptions;
using global::SmartLogistics.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.Common.Models;
using System.Net;
using System.Text.Json;

namespace SmartLogistics.API.Middleware
{
   
    /// <summary>
    /// Global exception handler middleware.
    /// Catches all unhandled exceptions and returns a consistent error response.
    /// Maps domain exceptions to appropriate HTTP status codes.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message, errors) = exception switch
            {
                NotFoundException e => (HttpStatusCode.NotFound, e.Message, (List<string>?)null),
                UnauthorizedException e => (HttpStatusCode.Unauthorized, e.Message, null),
                ForbiddenException e => (HttpStatusCode.Forbidden, e.Message, null),
                BusinessRuleException e => (HttpStatusCode.UnprocessableEntity, e.Message, null),
                Application.Common.Exceptions.ValidationException e =>
                    (HttpStatusCode.BadRequest, "Validation failed.",
                     e.Errors.SelectMany(kvp => kvp.Value.Select(v => $"{kvp.Key}: {v}")).ToList()),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
            };

            // Log 5xx errors as Error, others as Warning
            if (statusCode == HttpStatusCode.InternalServerError)
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            else
                _logger.LogWarning("Handled exception [{StatusCode}]: {Message}", (int)statusCode, exception.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<object>.Fail(message, (int)statusCode, errors);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, _jsonOptions));
        }
    }

    /// <summary>
    /// HTTP request/response logging middleware.
    /// Logs method, path, status code, and duration for every request.
    /// </summary>
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
            var start = DateTime.UtcNow;
            var traceId = context.TraceIdentifier;

            _logger.LogInformation("→ {Method} {Path} | TraceId: {TraceId}",
                context.Request.Method, context.Request.Path, traceId);

            await _next(context);

            var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
            var level = context.Response.StatusCode >= 500 ? Microsoft.Extensions.Logging.LogLevel.Error
                      : context.Response.StatusCode >= 400 ? Microsoft.Extensions.Logging.LogLevel.Warning
                      : Microsoft.Extensions.Logging.LogLevel.Information;

            _logger.Log(level, "← {Method} {Path} → {StatusCode} | {Elapsed:F0}ms | TraceId: {TraceId}",
                context.Request.Method, context.Request.Path,
                context.Response.StatusCode, elapsed, traceId);
        }
    }
}
