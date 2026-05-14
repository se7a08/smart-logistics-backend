using System;

namespace SmartLogistics.Application.Common.Exceptions
{
    // Exception بيترمي لما تفشل عملية التحقق من الهوية (مثلاً توكن منتهي أو يوزر نيم غلط)
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException()
            : base("عفواً، يجب تسجيل الدخول أولاً للوصول إلى هذه البيانات.")
        {
        }

        public UnauthorizedException(string message)
            : base(message)
        {
        }
    }
}