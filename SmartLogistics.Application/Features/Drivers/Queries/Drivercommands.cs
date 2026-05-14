using AutoMapper;
using MediatR;
using SmartLogistics.Application.DTOs.Drivers;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Drivers.Queries
{
    // --- Get Driver Tasks ---
    // Retrieves all active shipments assigned to a specific driver
    public record GetDriverTasksQuery(Guid DriverId) : IRequest<List<DriverTaskDto>>;

    public class GetDriverTasksQueryHandler : IRequestHandler<GetDriverTasksQuery, List<DriverTaskDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetDriverTasksQueryHandler(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<List<DriverTaskDto>> Handle(GetDriverTasksQuery query, CancellationToken ct)
        {
            // Fetch shipments that are assigned to this driver and still in progress
            var shipments = await _uow.Repository<Shipment>()
                .FindAsync(s => s.DriverId == query.DriverId &&
                                s.Status != ShipmentStatus.Delivered &&
                                s.Status != ShipmentStatus.Cancelled &&
                                !s.IsDeleted, ct);

            // Map the collection of Shipment entities to a list of DriverTaskDto
            return shipments.Select(s => _mapper.Map<DriverTaskDto>(s)).ToList();
        }
    }
}