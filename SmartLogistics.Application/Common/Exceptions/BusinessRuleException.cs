using System;

namespace SmartLogistics.Application.Common.Exceptions
{
    // Exception خاص بمخالفة قواعد العمل (مثلاً: سواق بيحاول يستلم شحنة وهي أصلاً مع سواق تاني)
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message)
        {
        }
    }
}