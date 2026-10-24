using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartLogistics.Application.DTOs.Shipments;
using SmartLogistics.Application.Features.Shipments;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;


namespace SmartLogistics.Application.Features.Drivers.Queries
{
    public record GetDriverActiveShipmentsQuery(Guid DriverId) : IRequest<List<ShipmentDto>>;

    public class GetDriverActiveShipmentsQueryHandler : IRequestHandler<GetDriverActiveShipmentsQuery, List<ShipmentDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetDriverActiveShipmentsQueryHandler(IUnitOfWork uow) => _uow = uow;

        public Task<List<ShipmentDto>> Handle(GetDriverActiveShipmentsQuery q, CancellationToken ct) =>
            _uow.Repository<Shipment>().Query()
                .Where(s => s.DriverId == q.DriverId
                         && s.Status != ShipmentStatus.Delivered
                         && s.Status != ShipmentStatus.Cancelled)
                .OrderBy(s => s.EstimatedDelivery)
                .Select(ShipmentProjections.ToDto)
                .ToListAsync(ct);
    }
}
