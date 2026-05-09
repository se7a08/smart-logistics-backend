using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::SmartLogistics.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QRCoder;
using SmartLogistics.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SmartLogistics.Infrastructure.Services.QRCode
{
   
    /// <summary>
    /// QR code generation and validation service.
    /// Encodes shipment ID with an HMAC signature to prevent forgery.
    /// </summary>
    public class QrCodeService : IQrCodeService
    {
        private readonly IConfiguration _config;

        public QrCodeService(IConfiguration config) => _config = config;

        public string GenerateQrCode(Guid shipmentId)
        {
            var payload = $"SL:{shipmentId}";
            var signature = ComputeHmac(payload);
            return $"{payload}:{signature}";
        }

        public bool ValidateQrCode(string qrCode, Guid shipmentId)
        {
            if (string.IsNullOrEmpty(qrCode)) return false;

            var parts = qrCode.Split(':');
            if (parts.Length != 3) return false;

            var expectedPayload = $"SL:{shipmentId}";
            var payloadPart = $"{parts[0]}:{parts[1]}";

            if (payloadPart != expectedPayload) return false;

            var expectedSig = ComputeHmac(expectedPayload);
            return parts[2] == expectedSig;
        }

        public byte[] GenerateQrCodeImage(string data)
        {
            using var generator = new QRCodeGenerator();
            var qrData = generator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            return qrCode.GetGraphic(10);
        }

        private string ComputeHmac(string data)
        {
            var key = Encoding.UTF8.GetBytes(_config["QrCode:Secret"] ?? "default-qr-secret-key");
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash)[..16]; // Truncate for URL safety
        }
    }
}
