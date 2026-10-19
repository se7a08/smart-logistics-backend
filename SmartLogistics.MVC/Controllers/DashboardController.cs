using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Application.Features.Dashboard.Queries;

namespace SmartLogistics.MVC.Controllers
{
    [Authorize(Roles = "Admin,WarehouseManager")]
    public class DashboardController : Controller
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator) => _mediator = mediator;

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var stats = await _mediator.Send(new GetDashboardStatsQuery(), ct);
            return View(stats);
        }
    }
}
