namespace SmartLogistics.Application.DTOs.Auth
{
    // Represents a simplified view of the user profile
    public record UserDto(
        Guid Id,
        string FullName,
        string Email,
        string PhoneNumber,
        string Role,
        bool IsActive,
        string? LicenseNumber,
        string? VehiclePlate,
        DateTime CreatedAt
    );
}