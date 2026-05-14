namespace SmartLogistics.Application.DTOs.Warehouses
{
    public record WarehouseStatisticsDto(
         Guid WarehouseId,
         string WarehouseName,
         int TotalShipments,
         int PendingShipments,
         int InTransitShipments,
         int DeliveredShipments,
         int CancelledShipments,
         decimal OccupancyPercentage // Logic: (Used Capacity / Total Capacity) * 100
     );
}

