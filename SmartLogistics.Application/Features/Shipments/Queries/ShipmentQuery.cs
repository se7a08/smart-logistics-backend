using Microsoft.EntityFrameworkCore;
using AutoMapper;
using MediatR;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.Common.Models;
using SmartLogistics.Application.DTOs.Shipments;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Shipments.Queries
{
    
    public record GetShipmentByIdQuery(Guid Id) : IRequest<ShipmentDto>;

    public class GetShipmentByIdQueryHandler : IRequestHandler<GetShipmentByIdQuery, ShipmentDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetShipmentByIdQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<ShipmentDto> Handle(GetShipmentByIdQuery query, CancellationToken ct)
        {
            return await _uow.Repository<Shipment>().Query()
                .Where(s => s.Id == query.Id)
                .Select(s => new ShipmentDto(
                    s.Id, s.TrackingNumber, s.QrCode, s.RecipientName, s.RecipientPhone, s.RecipientEmail,
                    s.DeliveryAddress, s.DeliveryLatitude, s.DeliveryLongitude, s.Weight, s.Description,
                    s.DeclaredValue, s.IsFragile, s.Status.ToString(), s.PickedUpAt, s.DeliveredAt,
                    s.EstimatedDelivery, s.QrVerified, s.DeliveryNotes, s.DriverId,
                    s.Driver != null ? s.Driver.FullName : "Not Assigned",
                    s.OriginWarehouseId, s.OriginWarehouse.Name,
                    s.DestinationWarehouseId, s.DestinationWarehouse.Name,
                    s.CreatedAt))
                .FirstOrDefaultAsync(ct)
                ?? throw new NotFoundException("Shipment", query.Id);
        }
    }

    public record GetAllShipmentsQuery(QueryParameters Params, string? StatusFilter) : IRequest<PaginatedList<ShipmentDto>>;

    public class GetAllShipmentsQueryHandler : IRequestHandler<GetAllShipmentsQuery, PaginatedList<ShipmentDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetAllShipmentsQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PaginatedList<ShipmentDto>> Handle(GetAllShipmentsQuery query, CancellationToken ct)
        {
            var q = _uow.Repository<Shipment>().Query();

            if (!string.IsNullOrEmpty(query.StatusFilter) &&
                Enum.TryParse<ShipmentStatus>(query.StatusFilter, true, out var status))
                q = q.Where(s => s.Status == status);

            if (!string.IsNullOrEmpty(query.Params.SearchTerm))
            {
                var search = query.Params.SearchTerm.ToLower();
                q = q.Where(s => s.TrackingNumber.ToLower().Contains(search)
                              || s.RecipientName.ToLower().Contains(search));
            }

            var totalCount = await q.CountAsync(ct);

            var items = await q
                .OrderByDescending(s => s.CreatedAt)
                .Skip((query.Params.PageNumber - 1) * query.Params.PageSize)
                .Take(query.Params.PageSize)
                .Select(s => new ShipmentDto(
                    s.Id, s.TrackingNumber, s.QrCode, s.RecipientName, s.RecipientPhone, s.RecipientEmail,
                    s.DeliveryAddress, s.DeliveryLatitude, s.DeliveryLongitude, s.Weight, s.Description,
                    s.DeclaredValue, s.IsFragile, s.Status.ToString(), s.PickedUpAt, s.DeliveredAt,
                    s.EstimatedDelivery, s.QrVerified, s.DeliveryNotes, s.DriverId,
                    s.Driver != null ? s.Driver.FullName : "Not Assigned",
                    s.OriginWarehouseId, s.OriginWarehouse.Name,
                    s.DestinationWarehouseId, s.DestinationWarehouse.Name,
                    s.CreatedAt))
                .ToListAsync(ct);

            return PaginatedList<ShipmentDto>.Create(items, totalCount, query.Params.PageNumber, query.Params.PageSize);
        }
    }

    public record GetShipmentHistoryQuery(Guid ShipmentId) : IRequest<List<ShipmentStatusHistoryDto>>;

    public class GetShipmentHistoryQueryHandler : IRequestHandler<GetShipmentHistoryQuery, List<ShipmentStatusHistoryDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetShipmentHistoryQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<List<ShipmentStatusHistoryDto>> Handle(GetShipmentHistoryQuery query, CancellationToken ct)
        {
            var history = await _uow.Repository<ShipmentStatusHistory>()
                .FindAsync(h => h.ShipmentId == query.ShipmentId, ct);

            return history
                .OrderBy(h => h.CreatedAt)
                .Select(h => _mapper.Map<ShipmentStatusHistoryDto>(h))
                .ToList();
        }
    }
}