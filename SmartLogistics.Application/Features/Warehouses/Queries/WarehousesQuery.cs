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

namespace SmartLogistics.Application.Features.Warehouses.Queries
{

    // ─── Get Warehouse Statistics ───────────────────────────────────────────────

    public record GetWarehouseStatsQuery(Guid WarehouseId) : IRequest<WarehouseStatisticsDto>;

    public class GetWarehouseStatsQueryHandler : IRequestHandler<GetWarehouseStatsQuery, WarehouseStatisticsDto>
    {
        private readonly IUnitOfWork _uow;

        public GetWarehouseStatsQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<WarehouseStatisticsDto> Handle(GetWarehouseStatsQuery query, CancellationToken ct)
        {
            var warehouse = await _uow.Repository<Warehouse>()
                .GetByIdAsync(query.WarehouseId, ct)
                ?? throw new NotFoundException("Warehouse", query.WarehouseId);

            var shipments = await _uow.Repository<Shipment>()
                .FindAsync(s => s.OriginWarehouseId == query.WarehouseId && !s.IsDeleted, ct);

            var total = shipments.Count;
            return new WarehouseStatisticsDto(
                warehouse.Id,
                warehouse.Name,
                total,
                shipments.Count(s => s.Status == ShipmentStatus.Pending),
                shipments.Count(s => s.Status == ShipmentStatus.InTransit),
                shipments.Count(s => s.Status == ShipmentStatus.Delivered),
                shipments.Count(s => s.Status == ShipmentStatus.Cancelled),
                warehouse.Capacity > 0 ? Math.Round((decimal)total / warehouse.Capacity * 100, 2) : 0
            );
        }
    }
}
