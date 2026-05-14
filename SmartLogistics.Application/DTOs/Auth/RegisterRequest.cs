using SmartLogistics.Domain.Enums;

namespace SmartLogistics.Application.DTOs.Auth
{
    // Data required to create a new account in the system
    public record RegisterRequest(
        string FullName,
        string Email,
        string Password,
        string PhoneNumber,
        UserRole Role, // Can be Admin, Driver, or Customer
        string? LicenseNumber, // Required if role is Driver
        string? VehiclePlate   // Required if role is Driver
    );
}