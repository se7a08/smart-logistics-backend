using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Application.DTOs.Warehouses
{
    public record CreateWarehouseRequest(
        string Name,
        string Code,
        string Address,
        string City,
        string Country,
        double Latitude,
        double Longitude,
        int Capacity,
        string ManagerName,
        string ManagerPhone
    );

    public record UpdateWarehouseRequest(
        string Name,
        string Address,
        string City,
        string Country,
        double Latitude,
        double Longitude,
        int Capacity,
        string ManagerName,
        string ManagerPhone,
        bool IsActive
    );

    public record WarehouseDto(
        Guid Id,
        string Name,
        string Code,
        string Address,
        string City,
        string Country,
        double Latitude,
        double Longitude,
        int Capacity,
        bool IsActive,
        string ManagerName,
        string ManagerPhone,
        DateTime CreatedAt
    );

    public record WarehouseStatisticsDto(
        Guid WarehouseId,
        string WarehouseName,
        int TotalShipments,
        int PendingShipments,
        int InTransitShipments,
        int DeliveredShipments,
        int CancelledShipments,
        decimal OccupancyPercentage
    );
}

