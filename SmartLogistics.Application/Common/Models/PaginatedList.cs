using System;
using System.Collections.Generic;

namespace SmartLogistics.Application.Common.Models
{
    // كلاس مسؤول عن تقسيم البيانات لصفحات (Pagination) بدل ما نبعت كل الداتا مرة واحدة
    public class PaginatedList<T>
    {
        // العناصر اللي موجودة في الصفحة الحالية
        public List<T> Items { get; set; } = new List<T>();

        // إجمالي عدد العناصر في الداتا بيز كلها
        public int TotalCount { get; set; }

        // رقم الصفحة الحالية
        public int PageNumber { get; set; }

        // عدد العناصر في كل صفحة
        public int PageSize { get; set; }

        // حساب إجمالي عدد الصفحات تلقائياً
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        // خصائص مساعدة عشان نعرف فيه صفحات قبل أو بعد ولا لأ
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        // ميثود مساعدة لإنشاء النسخة دي من الكلاس بشكل أسهل
        public static PaginatedList<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            return new PaginatedList<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}