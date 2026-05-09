using AutoMapper;
using MediatR;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.DTOs.Drivers;
using SmartLogistics.Application.DTOs.Warehouses;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLogistics.Application.Features.Drivers.Queries
{

    // ─── Get Driver Tasks ───────────────────────────────────────────────────────

    public record GetDriverTasksQuery(Guid DriverId) : IRequest<List<DriverTaskDto>>;

    public class GetDriverTasksQueryHandler : IRequestHandler<GetDriverTasksQuery, List<DriverTaskDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GetDriverTasksQueryHandler(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        public async Task<List<DriverTaskDto>> Handle(GetDriverTasksQuery query, CancellationToken ct)
        {
            var shipments = await _uow.Repository<Shipment>()
                .FindAsync(s => s.DriverId == query.DriverId &&
                                s.Status != ShipmentStatus.Delivered &&
                                s.Status != ShipmentStatus.Cancelled &&
                                !s.IsDeleted, ct);

            return shipments.Select(_mapper.Map<DriverTaskDto>).ToList();
        }
    }

  
}
