using System;

namespace SmartLogistics.Application.Common.Exceptions
{
    // Exception بيترمي لما المستخدم يحاول يعمل حاجة مش مسموحة للصلاحيات بتاعته (Role)
    public class ForbiddenException : Exception
    {
        public ForbiddenException()
            : base("Access denied. You do not have permission to perform this action.")
        {
        }

        public ForbiddenException(string message)
            : base(message)
        {
        }
    }
}