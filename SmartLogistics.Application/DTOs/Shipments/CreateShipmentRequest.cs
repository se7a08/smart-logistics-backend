using SmartLogistics.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Application.DTOs.Shipments
{
    public record CreateShipmentRequest(
        string RecipientName,
        string RecipientPhone,
        string RecipientEmail,
        string DeliveryAddress,
        double DeliveryLatitude,
        double DeliveryLongitude,
        decimal Weight,
        string Description,
        decimal DeclaredValue,
        bool IsFragile,
        Guid OriginWarehouseId,
        Guid DestinationWarehouseId,
        DateTime? EstimatedDelivery
    );

    public record UpdateShipmentStatusRequest(
        ShipmentStatus Status,
        string? Notes,
        double? Latitude,
        double? Longitude
    );

    public record AssignDriverRequest(Guid DriverId);

    public record ShipmentDto(
        Guid Id,
        string TrackingNumber,
        string QrCode,
        string RecipientName,
        string RecipientPhone,
        string RecipientEmail,
        string DeliveryAddress,
        double DeliveryLatitude,
        double DeliveryLongitude,
        decimal Weight,
        string Description,
        decimal DeclaredValue,
        bool IsFragile,
        string Status,
        DateTime? PickedUpAt,
        DateTime? DeliveredAt,
        DateTime? EstimatedDelivery,
        bool QrVerified,
        string? DeliveryNotes,
        Guid? DriverId,
        string? DriverName,
        Guid OriginWarehouseId,
        string OriginWarehouseName,
        Guid DestinationWarehouseId,
        string DestinationWarehouseName,
        DateTime CreatedAt
    );

    public record ShipmentStatusHistoryDto(
        Guid Id,
        string Status,
        string Notes,
        double? Latitude,
        double? Longitude,
        DateTime CreatedAt
    );

    public record ScanQrRequest(string QrCode);

}

