using System;

namespace SmartLogistics.Application.Common.Exceptions
{
    // Exception بيترمي لما تفشل عملية التحقق من الهوية (مثلاً توكن منتهي أو يوزر نيم غلط)
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