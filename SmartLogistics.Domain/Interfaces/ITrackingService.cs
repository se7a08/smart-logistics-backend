using System;
using System.Threading.Tasks;

namespace SmartLogistics.Domain.Interfaces
{
    // واجهة خدمة التتبع اللحظي (SignalR) عشان نبعت تحديثات للمستخدمين فوراً
    public interface ITrackingService
    {
        // إرسال موقع السواق الحالي لكل المشرفين (Admins)
        Task BroadcastDriverLocationAsync(Guid driverId, double lat, double lng);

        // إرسال إشعار لحظي لما حالة الشحنة تتغير (مثلاً: تم الاستلام)
        Task NotifyShipmentStatusChangeAsync(Guid shipmentId, string status);

        // إرسال أي تحديثات عامة للوحة تحكم الأدمن
        Task NotifyAdminDashboardAsync(string eventType, object data);
    }
}