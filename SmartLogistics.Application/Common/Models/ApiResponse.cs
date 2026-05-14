using System;
using System.Collections.Generic;

namespace SmartLogistics.Application.Common.Models
{
    // A unified wrapper class for all API responses to simplify integration for the mobile team
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
        public int StatusCode { get; set; }

        // Standard success response
        public static ApiResponse<T> Ok(T data, string message = "Operation completed successfully")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = 200
            };
        }

        // Response for successful resource creation
        public static ApiResponse<T> Created(T data, string message = "Resource added successfully")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = 201
            };
        }

        // Response for operation failure (e.g., Bad Request)
        public static ApiResponse<T> Fail(string message, int statusCode = 400, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = errors
            };
        }

        // Response when the requested item is not found
        public static ApiResponse<T> NotFound(string message = "The requested resource was not found")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = 404
            };
        }

        // Response for unauthorized access
        public static ApiResponse<T> Unauthorized(string message = "Access denied. You are not authorized")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = 401
            };
        }
    }

    // Derived class for operations that don't return data (e.g., Delete or Update)
    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Ok(string message = "Success")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                StatusCode = 200
            };
        }

        public static new ApiResponse Fail(string message, int statusCode = 400, List<string>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = errors
            };
        }
    }
}