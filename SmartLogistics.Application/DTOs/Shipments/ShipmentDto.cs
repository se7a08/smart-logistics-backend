namespace SmartLogistics.Application.DTOs.Shipments
{
    // Complete details of a shipment used across the system
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
}