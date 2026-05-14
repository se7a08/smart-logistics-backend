using System;
using System.Collections.Generic;

namespace SmartLogistics.Application.Common.Models
{
    // كلاس موحد للرد على أي Request (Wrapper) عشان نسهل الشغل على بتاع الـ Mobile
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
        public int StatusCode { get; set; }

        // رد في حالة النجاح
        public static ApiResponse<T> Ok(T data, string message = "تمت العملية بنجاح")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = 200
            };
        }

        // رد في حالة إنشاء سجل جديد (Created)
        public static ApiResponse<T> Created(T data, string message = "تم الإضافة بنجاح")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = 201
            };
        }

        // رد في حالة الفشل (Bad Request مثلاً)
        public static ApiResponse<T> Fail(string message, int statusCode = 400, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = errors
            };
        }

        // رد في حالة عدم وجود البيانات
        public static ApiResponse<T> NotFound(string message = "العنصر المطلوب غير موجود")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = 404
            };
        }

        // رد في حالة عدم وجود صلاحية
        public static ApiResponse<T> Unauthorized(string message = "عفواً، غير مصرح لك بالوصول")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = 401
            };
        }
    }

    // كلاس فرعي للعمليات اللي مش بترجع داتا (زي الـ Delete أو الـ Update)
    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Ok(string message = "Success")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                StatusCode = 200
            };
        }

        public static new ApiResponse Fail(string message, int statusCode = 400, List<string>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = errors
            };
        }
    }
}