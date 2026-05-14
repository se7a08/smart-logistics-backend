using AutoMapper;
using SmartLogistics.Application.DTOs.Auth;
using SmartLogistics.Application.DTOs.Drivers;
using SmartLogistics.Application.DTOs.Shipments;
using SmartLogistics.Application.DTOs.Warehouses;
using SmartLogistics.Domain.Entities;

namespace SmartLogistics.Application.Common.Mappings
{
    // ملف إعدادات الـ AutoMapper لتحويل الـ Entities لـ DTOs والعكس
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // إعدادات تحويل بيانات المستخدم
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            // إعدادات تحويل الشحنات - بنربط أسماء المستودعات والسواقين بدل الـ IDs بس
            CreateMap<Shipment, ShipmentDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.DriverName, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.FullName : "لم يتم التعيين"))
                .ForMember(dest => dest.OriginWarehouseName, opt => opt.MapFrom(src => src.OriginWarehouse.Name))
                .ForMember(dest => dest.DestinationWarehouseName, opt => opt.MapFrom(src => src.DestinationWarehouse.Name));

            // تحويل تاريخ حالات الشحنة
            CreateMap<ShipmentStatusHistory, ShipmentStatusHistoryDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // تحويل بيانات المخازن/المستودعات
            CreateMap<Warehouse, WarehouseDto>();

            // تحويل بيانات مواقع السواقين
            CreateMap<DriverLocation, DriverLocationDto>()
                .ForMember(dest => dest.DriverName, opt => opt.MapFrom(src => src.Driver.FullName));

            // تحويل الشحنة لشكل "مهمة" للسواق (Driver Task)
            CreateMap<Shipment, DriverTaskDto>()
                .ForMember(dest => dest.ShipmentId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}