using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using QRCoder;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Infrastructure.Services.QRCode
{
    // الخدمة المسؤولة عن توليد وفحص الـ QR Code الخاص بالشحنات
    public class QrCodeService : IQrCodeService
    {
        private readonly IConfiguration _config;

        public QrCodeService(IConfiguration config)
        {
            _config = config;
        }

        // توليد نص الـ QR Code (عبارة عن معرف الشحنة + توقيع أمان)
        public string GenerateQrCode(Guid shipmentId)
        {
            string payload = $"SL-{shipmentId}"; // غيرنا : لـ - كنوع من التغيير البسيط
            string signature = ComputeHmac(payload);

            return $"{payload}:{signature}";
        }

        // التأكد إن الـ QR Code اللي اتعمل له سكان سليم ومش مزور
        public bool ValidateQrCode(string qrCode, Guid shipmentId)
        {
            if (string.IsNullOrEmpty(qrCode)) return false;

            // بنقسم النص لجزئين: البيانات والتوقيع
            var parts = qrCode.Split(':');
            if (parts.Length != 2) return false;

            string payloadPart = parts[0];
            string signaturePart = parts[1];

            // التأكد إن الـ ID اللي جوه الـ QR هو نفسه بتاع الشحنة المطلوبة
            string expectedPayload = $"SL-{shipmentId}";
            if (payloadPart != expectedPayload) return false;

            // إعادة حساب التوقيع ومقارنته باللي جاي في الـ QR
            string expectedSignature = ComputeHmac(expectedPayload);
            return signaturePart == expectedSignature;
        }

        // تحويل النص لصورة QR Code (بصيغة Byte Array عشان الموبايل يعرضها)
        public byte[] GenerateQrCodeImage(string data)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(15); // كبرنا الـ Size شوية لـ 15
                }
            }
        }

        // دالة داخلية لحساب التوقيع الرقمي (HMAC) لضمان عدم التلاعب
        private string ComputeHmac(string data)
        {
            var keyStr = _config["QrCode:Secret"] ?? "MySuperSecretKey123";
            var keyBytes = Encoding.UTF8.GetBytes(keyStr);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var hashBytes = hmac.ComputeHash(dataBytes);

                string fullHash = Convert.ToBase64String(hashBytes);
                // بناخد أول 15 حرف بس عشان ميكونش النص طويل زيادة في الـ QR
                return fullHash.Substring(0, 15);
            }
        }
    }
}