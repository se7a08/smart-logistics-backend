using System;

namespace SmartLogistics.Application.Common.Models
{
    // كلاس بيشيل معايير البحث والتقسيم (Pagination) اللي بتيجي مع الـ Request
    public class QueryParameters
    {
        private const int MaxPageSize = 100; // أقصى عدد عناصر مسموح به في الصفحة الواحدة
        private int _pageSize = 10;          // العدد الافتراضي هو 10

        // رقم الصفحة (بتبدأ من 1 بشكل افتراضي)
        public int PageNumber { get; set; } = 1;

        // حجم الصفحة مع التأكد إن المستخدم ميطلبش رقم خيالي يوقع السيرفر
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
            }
        }

        // كلمة البحث (زي اسم السواق أو رقم الشحنة)
        public string? SearchTerm { get; set; }

        // اسم الحقل اللي عايزين نرتب بناءً عليه (مثلاً CreatedAt)
        public string? SortBy { get; set; }

        // هل الترتيب تنازلي؟ (الافتراضي لا، يعني تصاعدي)
        public bool SortDescending { get; set; } = false;
    }
}