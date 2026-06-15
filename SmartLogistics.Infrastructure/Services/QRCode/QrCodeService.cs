using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using QRCoder;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Infrastructure.Services.QRCode
{
    
    public class QrCodeService : IQrCodeService
    {
        private readonly IConfiguration _config;

        public QrCodeService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateQrCode(Guid shipmentId)
        {
            string payload = $"SL-{shipmentId}"; 
            string signature = ComputeHmac(payload);

            return $"{payload}:{signature}";
        }

        
        public bool ValidateQrCode(string qrCode, Guid shipmentId)
        {
            if (string.IsNullOrEmpty(qrCode)) return false;

            var parts = qrCode.Split(':');
            if (parts.Length != 2) return false;

            string payloadPart = parts[0];
            string signaturePart = parts[1];

            
            string expectedPayload = $"SL-{shipmentId}";
            if (payloadPart != expectedPayload) return false;

          
            string expectedSignature = ComputeHmac(expectedPayload);
            return signaturePart == expectedSignature;
        }

       
        public byte[] GenerateQrCodeImage(string data)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(15); 
                }
            }
        }

        
        private string ComputeHmac(string data)
        {
            var keyStr = _config["QrCode:Secret"] ?? "MySuperSecretKey123";
            var keyBytes = Encoding.UTF8.GetBytes(keyStr);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var hashBytes = hmac.ComputeHash(dataBytes);

                string fullHash = Convert.ToBase64String(hashBytes);
                
                return fullHash.Substring(0, 15);
            }
        }
    }
}