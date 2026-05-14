using SmartLogistics.Domain.Enums;

namespace SmartLogistics.Application.DTOs.Shipments
{
    public record UpdateShipmentStatusRequest(
        ShipmentStatus Status,
        string? Notes,
        double? Latitude,
        double? Longitude
    );

}

