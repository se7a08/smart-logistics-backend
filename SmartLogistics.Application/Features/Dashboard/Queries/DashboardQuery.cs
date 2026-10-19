using MediatR;
using SmartLogistics.Application.DTOs.Dashboard;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;


namespace SmartLogistics.Application.Features.Dashboard.Queries
{
    public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
    {
        private readonly IUnitOfWork _uow;

        public GetDashboardStatsQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery query, CancellationToken ct)
        {
            var shipments = _uow.Repository<Shipment>();
            var now = DateTime.UtcNow;
            var todayStart = now.Date;

            return new DashboardStatsDto
            {
                TotalShipments = await shipments.CountAsync(null, ct),
                CreatedToday = await shipments.CountAsync(s => s.CreatedAt >= todayStart, ct),
                Pending = await shipments.CountAsync(s => s.Status == ShipmentStatus.Pending, ct),
                InProgress = await shipments.CountAsync(
                    s => s.Status == ShipmentStatus.PickedUp || s.Status == ShipmentStatus.InTransit, ct),
                Delivered = await shipments.CountAsync(s => s.Status == ShipmentStatus.Delivered, ct),
                Cancelled = await shipments.CountAsync(s => s.Status == ShipmentStatus.Cancelled, ct),
                Delayed = await shipments.CountAsync(
                    s => s.EstimatedDelivery != null
                         && s.EstimatedDelivery < now
                         && s.Status != ShipmentStatus.Delivered
                         && s.Status != ShipmentStatus.Cancelled, ct),
                ActiveDrivers = await _uow.Repository<User>().CountAsync(
                    u => u.Role == UserRole.Driver && u.IsActive, ct),
                ActiveWarehouses = await _uow.Repository<Warehouse>().CountAsync(w => w.IsActive, ct)
            };
        }
    }
}
