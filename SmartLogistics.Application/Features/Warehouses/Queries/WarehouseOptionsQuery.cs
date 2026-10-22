using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Interfaces;


namespace SmartLogistics.Application.Features.Warehouses.Queries
{
    public record WarehouseOptionDto(Guid Id, string Name);

    public record GetWarehouseOptionsQuery : IRequest<List<WarehouseOptionDto>>;

    public class GetWarehouseOptionsQueryHandler : IRequestHandler<GetWarehouseOptionsQuery, List<WarehouseOptionDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetWarehouseOptionsQueryHandler(IUnitOfWork uow) => _uow = uow;

        public Task<List<WarehouseOptionDto>> Handle(GetWarehouseOptionsQuery query, CancellationToken ct) =>
            _uow.Repository<Warehouse>().Query()
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .Select(w => new WarehouseOptionDto(w.Id, w.Name))
                .ToListAsync(ct);
    }
}
