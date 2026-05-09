using AutoMapper;
using MediatR;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.Common.Models;
using SmartLogistics.Application.DTOs.Shipments;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Application.Features.Shipments.Queries
{
    // ─── Get Shipment By ID ─────────────────────────────────────────────────────

    public record GetShipmentByIdQuery(Guid Id) : IRequest<ShipmentDto>;

    public class GetShipmentByIdQueryHandler : IRequestHandler<GetShipmentByIdQuery, ShipmentDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetShipmentByIdQueryHandler(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        public async Task<ShipmentDto> Handle(GetShipmentByIdQuery query, CancellationToken ct)
        {
            var shipment = await _uow.Repository<Shipment>()
                .FirstOrDefaultAsync(s => s.Id == query.Id && !s.IsDeleted, ct)
                ?? throw new NotFoundException("Shipment", query.Id);

            return _mapper.Map<ShipmentDto>(shipment);
        }
    }

    // ─── Get All Shipments ──────────────────────────────────────────────────────

    public record GetAllShipmentsQuery(Common.Models.QueryParameters Params, string? StatusFilter) : IRequest<PaginatedList<ShipmentDto>>;

    public class GetAllShipmentsQueryHandler : IRequestHandler<GetAllShipmentsQuery, PaginatedList<ShipmentDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetAllShipmentsQueryHandler(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        public async Task<PaginatedList<ShipmentDto>> Handle(GetAllShipmentsQuery query, CancellationToken ct)
        {
            var all = await _uow.Repository<Shipment>().FindAsync(s => !s.IsDeleted, ct);

            var filtered = all.AsQueryable();

            if (!string.IsNullOrEmpty(query.StatusFilter) &&
                Enum.TryParse<ShipmentStatus>(query.StatusFilter, true, out var status))
                filtered = filtered.Where(s => s.Status == status);

            if (!string.IsNullOrEmpty(query.Params.SearchTerm))
                filtered = filtered.Where(s =>
                    s.TrackingNumber.Contains(query.Params.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    s.RecipientName.Contains(query.Params.SearchTerm, StringComparison.OrdinalIgnoreCase));

            var totalCount = filtered.Count();
            var items = filtered
                .Skip((query.Params.PageNumber - 1) * query.Params.PageSize)
                .Take(query.Params.PageSize)
                .Select(_mapper.Map<ShipmentDto>)
                .ToList();

            return PaginatedList<ShipmentDto>.Create(items, totalCount, query.Params.PageNumber, query.Params.PageSize);
        }
    }

    // ─── Get Shipment Tracking History ─────────────────────────────────────────

    public record GetShipmentHistoryQuery(Guid ShipmentId) : IRequest<List<ShipmentStatusHistoryDto>>;

    public class GetShipmentHistoryQueryHandler : IRequestHandler<GetShipmentHistoryQuery, List<ShipmentStatusHistoryDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetShipmentHistoryQueryHandler(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        public async Task<List<ShipmentStatusHistoryDto>> Handle(GetShipmentHistoryQuery query, CancellationToken ct)
        {
            var history = await _uow.Repository<ShipmentStatusHistory>()
                .FindAsync(h => h.ShipmentId == query.ShipmentId, ct);

            return history.OrderBy(h => h.CreatedAt).Select(_mapper.Map<ShipmentStatusHistoryDto>).ToList();
        }
    }
}
