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
            var shipment = await _uow.Repository<Shipment>()
                .FirstOrDefaultAsync(s => s.Id == query.Id && !s.IsDeleted, ct)
                ?? throw new NotFoundException("Shipment", query.Id);

            return _mapper.Map<ShipmentDto>(shipment);
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
            
            var allShipments = await _uow.Repository<Shipment>().FindAsync(s => !s.IsDeleted, ct);
            var filtered = allShipments.AsQueryable();

            
            if (!string.IsNullOrEmpty(query.StatusFilter) &&
                Enum.TryParse<ShipmentStatus>(query.StatusFilter, true, out var status))
            {
                filtered = filtered.Where(s => s.Status == status);
            }

            if (!string.IsNullOrEmpty(query.Params.SearchTerm))
            {
                var search = query.Params.SearchTerm.ToLower();
                filtered = filtered.Where(s =>
                    s.TrackingNumber.ToLower().Contains(search) ||
                    s.RecipientName.ToLower().Contains(search));
            }

            var totalCount = filtered.Count();

            var items = filtered
                .OrderByDescending(s => s.CreatedAt) // Most recent first
                .Skip((query.Params.PageNumber - 1) * query.Params.PageSize)
                .Take(query.Params.PageSize)
                .Select(s => _mapper.Map<ShipmentDto>(s))
                .ToList();

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