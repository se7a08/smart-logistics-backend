using System;

namespace SmartLogistics.Domain.Interfaces
{
    // واجهة لخدمة التوكن (JWT) عشان عملية تسجيل الدخول وتأمين البيانات
    public interface IJwtService
    {
        // دالة بتعمل Token للمستخدم بناءً على بياناته
        string GenerateAccessToken(Guid userId, string email, string role);

        // دالة بتتأكد إن الـ Token سليم وبترجع الـ ID بتاع صاحب التوكن
        Guid? ValidateToken(string token);
    }
}