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

        public async Task BroadcastDriverLocationAsync(Guid driverId, double lat, double lng)
        {
            
            var locationData = new
            {
                driverId = driverId,
                lat = lat,
                lng = lng,
                time = DateTime.Now 
            };

            
            await _hubContext.Clients.Group("Admins")
                .SendAsync("DriverLocationUpdated", locationData);

            _logger.LogInformation($"Location updated for driver: {driverId}");
        }

        public async Task NotifyShipmentStatusChangeAsync(Guid shipmentId, string status)
        {
            var updateInfo = new
            {
                shipmentId = shipmentId,
                status = status,
                updateAt = DateTime.Now
            };

            await _hubContext.Clients.Group($"Shipment_{shipmentId}")
                .SendAsync("ShipmentStatusChanged", updateInfo);

            
            await _hubContext.Clients.Group("Admins")
                .SendAsync("ShipmentStatusChanged", updateInfo);
        }

        public async Task NotifyAdminDashboardAsync(string eventType, object data)
        {
            await _hubContext.Clients.Group("Admins")
                .SendAsync(eventType, data);
        }
    }
}