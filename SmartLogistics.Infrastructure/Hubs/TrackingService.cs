using global::SmartLogistics.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SmartLogistics.Infrastructure.Hubs
{
    public class TrackingService : ITrackingService
    {
        private readonly IHubContext<TrackingHub> _hubContext;
        private readonly ILogger<TrackingService> _logger;

        public TrackingService(IHubContext<TrackingHub> hubContext, ILogger<TrackingService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        // دالة لإرسال موقع السواق حالياً لكل الأدمنز
        public async Task BroadcastDriverLocationAsync(Guid driverId, double lat, double lng)
        {
            // بنجهز البيانات اللي هتروح للـ Frontend
            var locationData = new
            {
                driverId = driverId,
                lat = lat,
                lng = lng,
                time = DateTime.Now // استخدمنا Now العادية بدل UtcNow للتبسيط
            };

            // بنكلم الـ Hub يبعت للمجموعة اللي اسمها Admins
            await _hubContext.Clients.Group("Admins")
                .SendAsync("DriverLocationUpdated", locationData);

            _logger.LogInformation($"Location updated for driver: {driverId}");
        }

        // إشعار بتغير حالة الشحنة (مثلاً بقت In Transit)
        public async Task NotifyShipmentStatusChangeAsync(Guid shipmentId, string status)
        {
            var updateInfo = new
            {
                shipmentId = shipmentId,
                status = status,
                updateAt = DateTime.Now
            };

            // بنبعت للعملاء المتابعين للشحنة دي
            await _hubContext.Clients.Group($"Shipment_{shipmentId}")
                .SendAsync("ShipmentStatusChanged", updateInfo);

            // وبنبعت برضه للأدمن عشان يتابع من لوحة التحكم
            await _hubContext.Clients.Group("Admins")
                .SendAsync("ShipmentStatusChanged", updateInfo);
        }

        // إرسال أي أحداث تانية للوحة تحكم الأدمن
        public async Task NotifyAdminDashboardAsync(string eventType, object data)
        {
            await _hubContext.Clients.Group("Admins")
                .SendAsync(eventType, data);
        }
    }
}