using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Application.Common.Exceptions
{
    /// <summary>
    /// Thrown when a requested resource cannot be found.
    /// Maps to HTTP 404.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string name, object key)
            : base($"Entity '{name}' with key '{key}' was not found.") { }

        public NotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when validation of a request fails.
    /// Maps to HTTP 400 with validation error details.
    /// </summary>
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors;
        }
    }

    /// <summary>
    /// Thrown when a user attempts an unauthorized action.
    /// Maps to HTTP 403.
    /// </summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message = "You do not have permission to perform this action.")
            : base(message) { }
    }

    /// <summary>
    /// Thrown when authentication fails (bad credentials, expired token, etc.).
    /// Maps to HTTP 401.
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = "Authentication failed.")
            : base(message) { }
    }

    /// <summary>
    /// Thrown when a business rule is violated.
    /// Maps to HTTP 422.
    /// </summary>
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message) { }
    }
}

