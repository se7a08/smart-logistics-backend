using System;

namespace SmartLogistics.Application.Common.Exceptions
{
    
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message)
        {
        }
    }
}