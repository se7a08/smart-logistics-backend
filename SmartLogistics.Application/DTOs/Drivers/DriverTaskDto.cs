namespace SmartLogistics.Application.DTOs.Drivers
{
    // Details of a shipment assigned to a driver for delivery
    public record DriverTaskDto(
        Guid ShipmentId,
        string TrackingNumber,
        string RecipientName,
        string RecipientPhone,
        string DeliveryAddress,
        double DeliveryLatitude,  // For navigation in Google Maps
        double DeliveryLongitude, // For navigation in Google Maps
        string Status,
        DateTime? EstimatedDelivery
    );
}