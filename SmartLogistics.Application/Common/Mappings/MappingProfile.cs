using AutoMapper;
using SmartLogistics.Application.DTOs.Auth;
using SmartLogistics.Application.DTOs.Drivers;
using SmartLogistics.Application.DTOs.Shipments;
using SmartLogistics.Application.DTOs.Warehouses;
using SmartLogistics.Domain.Entities;

namespace SmartLogistics.Application.Common.Mappings
{
    
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            
            CreateMap<Shipment, ShipmentDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DriverName, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.FullName : "Not Assigned"))
                .ForMember(dest => dest.OriginWarehouseName, opt => opt.MapFrom(src => src.OriginWarehouse.Name))
                .ForMember(dest => dest.DestinationWarehouseName, opt => opt.MapFrom(src => src.DestinationWarehouse.Name));

            
            CreateMap<ShipmentStatusHistory, ShipmentStatusHistoryDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Warehouse, WarehouseDto>();

            
            CreateMap<DriverLocation, DriverLocationDto>()
                .ForMember(dest => dest.DriverName, opt => opt.MapFrom(src => src.Driver.FullName));

            
            CreateMap<Shipment, DriverTaskDto>()
                .ForMember(dest => dest.ShipmentId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}