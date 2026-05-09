using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Application.Common.Models
{

    /// <summary>
    /// Unified API response wrapper for consistent response structure across all endpoints.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
        public int StatusCode { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success") =>
            new() { Success = true, Message = message, Data = data, StatusCode = 200 };

        public static ApiResponse<T> Created(T data, string message = "Created successfully") =>
            new() { Success = true, Message = message, Data = data, StatusCode = 201 };

        public static ApiResponse<T> Fail(string message, int statusCode = 400, List<string>? errors = null) =>
            new() { Success = false, Message = message, StatusCode = statusCode, Errors = errors };

        public static ApiResponse<T> NotFound(string message = "Resource not found") =>
            new() { Success = false, Message = message, StatusCode = 404 };

        public static ApiResponse<T> Unauthorized(string message = "Unauthorized") =>
            new() { Success = false, Message = message, StatusCode = 401 };
    }

    /// <summary>
    /// Generic non-typed response for operations that don't return data.
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Ok(string message = "Success") =>
            new() { Success = true, Message = message, StatusCode = 200 };

        public static new ApiResponse Fail(string message, int statusCode = 400, List<string>? errors = null) =>
            new() { Success = false, Message = message, StatusCode = statusCode, Errors = errors };
    }

    /// <summary>
    /// Pagination metadata for paginated list responses.
    /// </summary>
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public static PaginatedList<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize) =>
            new() { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
    }

    /// <summary>
    /// Common query parameters for pagination, filtering, and sorting.
    /// </summary>
    public class QueryParameters
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}

