using System;
using System.Collections.Generic;

namespace SmartLogistics.Application.Common.Exceptions
{
    // Exception مخصص لأخطاء المدخلات (التحقق من صحة البيانات)
    public class ValidationException : Exception
    {
        // قاموس بيشيل اسم الحقل اللي فيه مشكلة وقائمة بالأخطاء اللي تخصه
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException()
            : base("One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("Validation failed. The provided data is invalid.")
        {
            Errors = errors;
        }
    }
}