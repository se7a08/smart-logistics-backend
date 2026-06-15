using AutoMapper;
using MediatR;
using SmartLogistics.Application.DTOs.Drivers;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.Application.Features.Drivers.Queries
{
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
            var shipments = await _uow.Repository<Shipment>()
                .FindAsync(s => s.DriverId == query.DriverId &&
                                s.Status != ShipmentStatus.Delivered &&
                                s.Status != ShipmentStatus.Cancelled &&
                                !s.IsDeleted, ct);

            
            return shipments.Select(s => _mapper.Map<DriverTaskDto>(s)).ToList();
        }
    }
}