using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Domain.Interfaces
{
    /// <summary>
    /// Interface for JWT token generation and validation.
    /// </summary>
    public interface IJwtService
    {
        string GenerateAccessToken(Guid userId, string email, string role);
        Guid? ValidateToken(string token);
    }

    /// <summary>
    /// Interface for QR code generation and validation.
    /// </summary>
    public interface IQrCodeService
    {
        string GenerateQrCode(Guid shipmentId);
        bool ValidateQrCode(string qrCode, Guid shipmentId);
        byte[] GenerateQrCodeImage(string data);
    }

    /// <summary>
    /// Interface for Firebase Cloud Messaging push notifications.
    /// </summary>
    public interface INotificationService
    {
        Task SendToDeviceAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null);
        Task SendToTopicAsync(string topic, string title, string body, Dictionary<string, string>? data = null);
        Task SendToMultipleDevicesAsync(IEnumerable<string> fcmTokens, string title, string body, Dictionary<string, string>? data = null);
    }

    /// <summary>
    /// Interface for real-time SignalR tracking hub operations.
    /// </summary>
    public interface ITrackingService
    {
        Task BroadcastDriverLocationAsync(Guid driverId, double lat, double lng);
        Task NotifyShipmentStatusChangeAsync(Guid shipmentId, string status);
        Task NotifyAdminDashboardAsync(string eventType, object data);
    }

    /// <summary>
    /// Interface for password hashing operations.
    /// </summary>
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }
}

