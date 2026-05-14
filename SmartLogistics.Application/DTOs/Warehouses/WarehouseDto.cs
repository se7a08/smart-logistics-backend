namespace SmartLogistics.Application.DTOs.Warehouses
{
    // Full representation of a warehouse facility for the Admin dashboard
    public record WarehouseDto(
        Guid Id,
        string Name,
        string Code, // Unique identifier like WH-MIN-01
        string Address,
        string City,
        string Country,
        double Latitude,
        double Longitude,
        int Capacity, // Total storage capacity
        bool IsActive,
        string ManagerName,
        string ManagerPhone,
        DateTime CreatedAt
    );
}