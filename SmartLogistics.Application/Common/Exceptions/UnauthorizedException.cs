using System;

namespace SmartLogistics.Application.Common.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException()
            : base("Unauthorized access. Please log in first to access this data.")
        {
        }

        public UnauthorizedException(string message)
            : base(message)
        {
        }
    }
}