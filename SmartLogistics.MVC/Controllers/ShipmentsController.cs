using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.Common.Models;
using SmartLogistics.Application.Features.Shipments.Queries;
using SmartLogistics.Application.Features.Shipments.Commands;
using SmartLogistics.Application.Features.Warehouses.Queries;
using SmartLogistics.MVC.Models;
using SmartLogistics.Application.DTOs.Shipments;
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
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = new CreateShipmentViewModel
            {
                Warehouses = await _mediator.Send(new GetWarehouseOptionsQuery(), ct),
                EstimatedDelivery = DateTime.UtcNow.Date.AddDays(2)
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateShipmentViewModel vm, CancellationToken ct)
        {
            if (vm.OriginWarehouseId == vm.DestinationWarehouseId && vm.OriginWarehouseId != null)
                ModelState.AddModelError(nameof(vm.DestinationWarehouseId), "Origin and destination must be different.");

            if (ModelState.IsValid)
            {
                try
                {
                    var request = new CreateShipmentRequest(
                        vm.RecipientName, vm.RecipientPhone, vm.RecipientEmail, vm.DeliveryAddress,
                        vm.DeliveryLatitude, vm.DeliveryLongitude, vm.Weight, vm.Description,
                        vm.DeclaredValue, vm.IsFragile,
                        vm.OriginWarehouseId!.Value, vm.DestinationWarehouseId!.Value,
                        vm.EstimatedDelivery);

                    var created = await _mediator.Send(new CreateShipmentCommand(request), ct);
                    TempData["Success"] = $"Shipment {created.TrackingNumber} created.";
                    return RedirectToAction(nameof(Details), new { id = created.Id });
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception ex) when (ex is NotFoundException or BusinessRuleException)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            vm.Warehouses = await _mediator.Send(new GetWarehouseOptionsQuery(), ct);
            return View(vm);
        }
    }
}
