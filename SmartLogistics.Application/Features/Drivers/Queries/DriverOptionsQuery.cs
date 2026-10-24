using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartLogistics.Domain.Entities;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;


namespace SmartLogistics.Application.Features.Drivers.Queries
{
    public record DriverOptionDto(Guid Id, string Name);

    public record GetDriverOptionsQuery : IRequest<List<DriverOptionDto>>;

    public class GetDriverOptionsQueryHandler : IRequestHandler<GetDriverOptionsQuery, List<DriverOptionDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetDriverOptionsQueryHandler(IUnitOfWork uow) => _uow = uow;

        public Task<List<DriverOptionDto>> Handle(GetDriverOptionsQuery query, CancellationToken ct) =>
            _uow.Repository<User>().Query()
                .Where(u => u.Role == UserRole.Driver && u.IsActive)
                .OrderBy(u => u.FullName)
                .Select(u => new DriverOptionDto(u.Id, u.FullName))
                .ToListAsync(ct);
    }
}
