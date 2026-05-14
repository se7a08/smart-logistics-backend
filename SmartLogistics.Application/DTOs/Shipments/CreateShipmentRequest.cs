namespace SmartLogistics.Application.DTOs.Shipments
{
    // Request model for creating a new shipment record
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
}