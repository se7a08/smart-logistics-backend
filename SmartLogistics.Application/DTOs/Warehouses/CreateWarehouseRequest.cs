using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Application.DTOs.Warehouses
{
    public record CreateWarehouseRequest(
        string Name,
        string Code,
        string Address,
        string City,
        string Country,
        double Latitude,
        double Longitude,
        int Capacity,
        string ManagerName,
        string ManagerPhone
    );
}

