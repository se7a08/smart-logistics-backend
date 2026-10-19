
namespace SmartLogistics.Application.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public int TotalShipments { get; set; }
        public int CreatedToday { get; set; }
        public int Pending { get; set; }
        public int InProgress { get; set; }   // PickedUp + InTransit
        public int Delivered { get; set; }
        public int Cancelled { get; set; }
        public int Delayed { get; set; }
        public int ActiveDrivers { get; set; }
        public int ActiveWarehouses { get; set; }
    }
}
