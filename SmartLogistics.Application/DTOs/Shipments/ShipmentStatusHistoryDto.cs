namespace SmartLogistics.Application.DTOs.Shipments
{
    public record ShipmentStatusHistoryDto(
        Guid Id,
        string Status,
        string Notes,
        double? Latitude,
        double? Longitude,
        DateTime CreatedAt
    );

}

