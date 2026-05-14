namespace SmartLogistics.Application.DTOs.Drivers
{
    // Data used to broadcast the driver's real-time position on the map
    public record DriverLocationDto(
        Guid DriverId,
        string DriverName,
        double Latitude,
        double Longitude,
        double? Speed,   // Speed in km/h
        double? Heading, // Direction (0-360 degrees)
        DateTime RecordedAt
    );
}