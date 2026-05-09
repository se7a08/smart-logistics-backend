using AutoMapper;
using SmartLogistics.Application.DTOs.Auth;
using SmartLogistics.Application.DTOs.Drivers;
using SmartLogistics.Application.DTOs.Shipments;
using SmartLogistics.Application.DTOs.Warehouses;
using SmartLogistics.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Application.Common.Mappings
{
    /// <summary>
    /// AutoMapper profile defining all entity-to-DTO mappings.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));

            // Shipment mappings
            CreateMap<Shipment, ShipmentDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.DriverName, o => o.MapFrom(s => s.Driver != null ? s.Driver.FullName : null))
                .ForMember(d => d.OriginWarehouseName, o => o.MapFrom(s => s.OriginWarehouse.Name))
                .ForMember(d => d.DestinationWarehouseName, o => o.MapFrom(s => s.DestinationWarehouse.Name));

            CreateMap<ShipmentStatusHistory, ShipmentStatusHistoryDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            // Warehouse mappings
            CreateMap<Warehouse, WarehouseDto>();

            // Driver location mappings
            CreateMap<DriverLocation, DriverLocationDto>()
                .ForMember(d => d.DriverName, o => o.MapFrom(s => s.Driver.FullName));

            // Driver task (shipment from driver perspective)
            CreateMap<Shipment, DriverTaskDto>()
                .ForMember(d => d.ShipmentId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
        }
    }
}

