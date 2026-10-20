using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.Common.Models;
using SmartLogistics.Application.Features.Shipments.Queries;

namespace SmartLogistics.MVC.Controllers
{
    [Authorize(Roles = "Admin,WarehouseManager")]
    public class ShipmentsController : Controller
    {
        private readonly IMediator _mediator;

        public ShipmentsController(IMediator mediator) => _mediator = mediator;

        public async Task<IActionResult> Index(string? search, string? status, int page = 1, CancellationToken ct = default)
        {
            var parameters = new QueryParameters
            {
                SearchTerm = search,
                PageNumber = page < 1 ? 1 : page,
                PageSize = 10
            };

            var result = await _mediator.Send(new GetAllShipmentsQuery(parameters, status), ct);

            ViewData["Search"] = search;
            ViewData["Status"] = status;
            return View(result);
        }

        public async Task<IActionResult> Details(Guid id, CancellationToken ct)
        {
            try
            {
                var shipment = await _mediator.Send(new GetShipmentByIdQuery(id), ct);
                var history = await _mediator.Send(new GetShipmentHistoryQuery(id), ct);
                ViewData["History"] = history;
                return View(shipment);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}
