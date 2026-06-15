using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.Common.Models;
using System.Net;
using System.Text.Json;

namespace SmartLogistics.API.Middleware
{
   
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

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
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred on the server.";
            List<string>? errors = null;

            if (exception is NotFoundException notFoundEx)
            {
                statusCode = HttpStatusCode.NotFound;
                message = notFoundEx.Message;
            }
            else if (exception is UnauthorizedException authEx)
            {
                statusCode = HttpStatusCode.Unauthorized;
                message = authEx.Message;
            }
            else if (exception is ForbiddenException forbiddenEx)
            {
                statusCode = HttpStatusCode.Forbidden;
                message = forbiddenEx.Message;
            }
            else if (exception is BusinessRuleException businessEx)
            {
                statusCode = HttpStatusCode.UnprocessableEntity;
                message = businessEx.Message;
            }
            else if (exception is SmartLogistics.Application.Common.Exceptions.ValidationException valEx)
            {
                statusCode = HttpStatusCode.BadRequest;
                message = "Input validation failed.";

                
                errors = valEx.Errors.SelectMany(x => x.Value.Select(err => $"{x.Key}: {err}")).ToList();
            }

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "Unhandled Exception: {Msg}", exception.Message);
            }
            else
            {
                _logger.LogWarning("Handled Exception: {Msg} (Status: {Code})", message, (int)statusCode);
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<object>.Fail(message, (int)statusCode, errors);

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var result = JsonSerializer.Serialize(response, jsonOptions);

            await context.Response.WriteAsync(result);
        }
    }
}