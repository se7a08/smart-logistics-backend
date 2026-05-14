namespace SmartLogistics.Application.DTOs.Drivers
{
    // Incoming request from mobile app to update driver's current coordinates
    public record UpdateLocationRequest(
        double Latitude,
        double Longitude,
        double? Speed,
        double? Heading,
        double? Accuracy // GPS accuracy in meters
    );
}