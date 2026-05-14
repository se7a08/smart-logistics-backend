using System;

namespace SmartLogistics.Application.Common.Exceptions
{
    // Exception بيترمي لما المستخدم يحاول يعمل حاجة مش مسموحة للصلاحيات بتاعته (Role)
    public class ForbiddenException : Exception
    {
        public ForbiddenException()
            : base("عفواً، ليس لديك صلاحية للقيام بهذا الإجراء.")
        {
        }

        public ForbiddenException(string message)
            : base(message)
        {
        }
    }
}