using MediatR;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.DTOs.Warehouses;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Warehouses.Queries
{
    // --- Get Warehouse Statistics ---
    // Provides a high-level overview of a facility's performance and current workload
    public record GetWarehouseStatsQuery(Guid WarehouseId) : IRequest<WarehouseStatisticsDto>;

    public class GetWarehouseStatsQueryHandler : IRequestHandler<GetWarehouseStatsQuery, WarehouseStatisticsDto>
    {
        private readonly IUnitOfWork _uow;

        public GetWarehouseStatsQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<WarehouseStatisticsDto> Handle(GetWarehouseStatsQuery query, CancellationToken ct)
        {
            // Human Touch: Providing a descriptive name for the missing entity
            var warehouse = await _uow.Repository<Warehouse>()
                .GetByIdAsync(query.WarehouseId, ct)
                ?? throw new NotFoundException("Distribution Center", query.WarehouseId);

            // Fetching active shipments associated with this origin hub
            var shipments = await _uow.Repository<Shipment>()
                   .FindAsync(s => s.OriginWarehouseId == query.WarehouseId && !s.IsDeleted);

            var totalShipments = shipments.Count;

            // Intelligence Logic: Calculate how much of the warehouse capacity is currently utilized
            decimal occupancy = 0;
            if (warehouse.Capacity > 0)
            {
                occupancy = Math.Round((decimal)totalShipments / warehouse.Capacity * 100, 2);
            }

            // Returning a meaningful summary for the Admin Dashboard
            return new WarehouseStatisticsDto(
                warehouse.Id,
                warehouse.Name,
                totalShipments,
                shipments.Count(s => s.Status == ShipmentStatus.Pending),
                shipments.Count(s => s.Status == ShipmentStatus.InTransit),
                shipments.Count(s => s.Status == ShipmentStatus.Delivered),
                shipments.Count(s => s.Status == ShipmentStatus.Cancelled),
                occupancy
            );
        }
    }
}