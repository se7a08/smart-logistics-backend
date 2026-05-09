using AutoMapper;
using MediatR;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.DTOs.Warehouses;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Application.Features.Warehouses.Commands
{
    // ─── Create Warehouse ───────────────────────────────────────────────────────

    public record CreateWarehouseCommand(CreateWarehouseRequest Request) : IRequest<WarehouseDto>;

    public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CreateWarehouseCommandHandler(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        public async Task<WarehouseDto> Handle(CreateWarehouseCommand command, CancellationToken ct)
        {
            var req = command.Request;

            if (await _uow.Repository<Warehouse>().AnyAsync(w => w.Code == req.Code, ct))
                throw new BusinessRuleException($"Warehouse with code '{req.Code}' already exists.");

            var warehouse = new Warehouse
            {
                Name = req.Name,
                Code = req.Code.ToUpper(),
                Address = req.Address,
                City = req.City,
                Country = req.Country,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                Capacity = req.Capacity,
                ManagerName = req.ManagerName,
                ManagerPhone = req.ManagerPhone
            };

            await _uow.Repository<Warehouse>().AddAsync(warehouse, ct);
            await _uow.SaveChangesAsync(ct);
            return _mapper.Map<WarehouseDto>(warehouse);
        }
    }

    // ─── Update Warehouse ───────────────────────────────────────────────────────

    public record UpdateWarehouseCommand(Guid Id, UpdateWarehouseRequest Request) : IRequest<WarehouseDto>;

    public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, WarehouseDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public UpdateWarehouseCommandHandler(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        public async Task<WarehouseDto> Handle(UpdateWarehouseCommand command, CancellationToken ct)
        {
            var warehouse = await _uow.Repository<Warehouse>()
                .FirstOrDefaultAsync(w => w.Id == command.Id && !w.IsDeleted, ct)
                ?? throw new NotFoundException("Warehouse", command.Id);

            var req = command.Request;
            warehouse.Name = req.Name;
            warehouse.Address = req.Address;
            warehouse.City = req.City;
            warehouse.Country = req.Country;
            warehouse.Latitude = req.Latitude;
            warehouse.Longitude = req.Longitude;
            warehouse.Capacity = req.Capacity;
            warehouse.ManagerName = req.ManagerName;
            warehouse.ManagerPhone = req.ManagerPhone;
            warehouse.IsActive = req.IsActive;
            warehouse.UpdatedAt = DateTime.UtcNow;

            _uow.Repository<Warehouse>().Update(warehouse);
            await _uow.SaveChangesAsync(ct);
            return _mapper.Map<WarehouseDto>(warehouse);
        }
    }

   
}
