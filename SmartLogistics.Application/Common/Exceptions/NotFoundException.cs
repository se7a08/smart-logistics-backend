using System;

namespace SmartLogistics.Application.Common.Exceptions
{
    // Exception بنستخدمه لما ندور على حاجة في الداتا بيز وم نلاقيهاش (زي شحنة أو مستخدم)
    public class NotFoundException : Exception
    {
        // الـ Constructor ده بنبعت له اسم الجدول والـ ID اللي كنا بندور عليه
        public NotFoundException(string name, object key)
            : base($"عفواً، الـ {name} اللي رقمه ({key}) مش موجود في السيستم.")
        {
        }

        // الـ Constructor ده لو عايزين نبعت رسالة معينة إحنا اللي كاتبينها
        public NotFoundException(string message) : base(message)
        {
        }
    }
}