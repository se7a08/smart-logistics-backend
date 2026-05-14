namespace SmartLogistics.Application.DTOs.Warehouses
{
    public record UpdateWarehouseRequest(
         string Name,
         string Address,
         string City,
         string Country,
         double Latitude,
         double Longitude,
         int Capacity,
         string ManagerName,
         string ManagerPhone,
         bool IsActive
     );
}

