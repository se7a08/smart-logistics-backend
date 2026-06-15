using System;
using System.Threading.Tasks;

namespace SmartLogistics.Domain.Interfaces
{
    public interface ITrackingService
    {
        Task BroadcastDriverLocationAsync(Guid driverId, double lat, double lng);

        Task NotifyShipmentStatusChangeAsync(Guid shipmentId, string status);

        Task NotifyAdminDashboardAsync(string eventType, object data);
    }
}