using AutoMapper;
using MediatR;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.DTOs.Warehouses;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Warehouses.Commands
{
    // --- Create Warehouse ---
    // Establishes a new logistics hub in the network
    public record CreateWarehouseCommand(CreateWarehouseRequest Request) : IRequest<WarehouseDto>;

    public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CreateWarehouseCommandHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<WarehouseDto> Handle(CreateWarehouseCommand command, CancellationToken ct)
        {
            var req = command.Request;

            // Human Touch: Check for unique codes to prevent administrative confusion
            var isCodeTaken = await _uow.Repository<Warehouse>().AnyAsync(w => w.Code == req.Code, ct);
            if (isCodeTaken)
            {
                throw new BusinessRuleException($"The warehouse code '{req.Code}' is already assigned to another facility. Please use a unique identifier.");
            }

            var warehouse = new Warehouse
            {
                Name = req.Name,
                Code = req.Code.ToUpper(), // Standardize code to uppercase for consistency
                Address = req.Address,
                City = req.City,
                Country = req.Country,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                Capacity = req.Capacity,
                ManagerName = req.ManagerName,
                ManagerPhone = req.ManagerPhone,
                IsActive = true, // New facilities are active by default
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Repository<Warehouse>().AddAsync(warehouse, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<WarehouseDto>(warehouse);
        }
    }

    // --- Update Warehouse ---
    // Modifies existing facility details or updates management contact information
    public record UpdateWarehouseCommand(Guid Id, UpdateWarehouseRequest Request) : IRequest<WarehouseDto>;

    public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, WarehouseDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public UpdateWarehouseCommandHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<WarehouseDto> Handle(UpdateWarehouseCommand command, CancellationToken ct)
        {
            var warehouse = await _uow.Repository<Warehouse>()
                .FirstOrDefaultAsync(w => w.Id == command.Id && !w.IsDeleted, ct)
                ?? throw new NotFoundException("Warehouse Hub", command.Id);

            var req = command.Request;

            // Updating core details with operational audit fields
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