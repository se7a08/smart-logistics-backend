using System;

namespace SmartLogistics.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
       
        public NotFoundException(string name, object key)
            : base($"{name} with ID ({key}) was not found in the system.")
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }
    }
}