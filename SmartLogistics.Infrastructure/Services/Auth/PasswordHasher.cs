using global::SmartLogistics.Domain.Interfaces;
// تأكد إنك ضايف مكتبة BCrypt.Net-Next في الـ NuGet

namespace SmartLogistics.Infrastructure.Services.Auth
{
    // خدمة تشفير كلمات السر باستخدام خوارزمية BCrypt
    public class PasswordHasher : IPasswordHasher
    {
        // معامل الصعوبة (Work Factor) - كل ما زاد زاد الوقت المطلوب للتشفير وزاد الأمان
        private const int SaltWorkFactor = 12;

        public string Hash(string password)
        {
            // توليد الـ Hash مع Salt تلقائي
            return BCrypt.Net.BCrypt.HashPassword(password, SaltWorkFactor);
        }

        public bool Verify(string password, string hash)
        {
            // مقارنة الباسورد المدخل بالـ Hash المتسيف في الداتا بيز
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                // في حالة وجود مشكلة في صيغة الـ Hash
                return false;
            }
        }
    }
}