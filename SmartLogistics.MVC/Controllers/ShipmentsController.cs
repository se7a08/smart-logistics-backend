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
using SmartLogistics.Application.Features.Drivers.Queries;
using SmartLogistics.Domain.Enums;
using SmartLogistics.Domain.Interfaces;
namespace SmartLogistics.MVC.Controllers
{
    [Authorize(Roles = "Admin,WarehouseManager")]
    public class ShipmentsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IQrCodeService _qr;

        public ShipmentsController(IMediator mediator, IQrCodeService qr)
        {
            _mediator = mediator;
            _qr = qr;
        }

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
                ViewData["Drivers"] = await _mediator.Send(new GetDriverOptionsQuery(), ct);
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
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDriver(Guid id, Guid driverId, CancellationToken ct)
        {
            try
            {
                await _mediator.Send(new AssignDriverCommand(id, driverId), ct);
                TempData["Success"] = "Driver assigned.";
            }
            catch (Exception ex) when (ex is NotFoundException or BusinessRuleException)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, ShipmentStatus status, string? notes, CancellationToken ct)
        {
            try
            {
                var userId = Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var g)
                    ? g : Guid.Empty;
                var request = new UpdateShipmentStatusRequest(status, notes, null, null);
                await _mediator.Send(new UpdateShipmentStatusCommand(id, userId, request), ct);
                TempData["Success"] = $"Status changed to {status}.";
            }
            catch (Exception ex) when (ex is NotFoundException or BusinessRuleException)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> QrImage(Guid id, CancellationToken ct)
        {
            try
            {
                var shipment = await _mediator.Send(new GetShipmentByIdQuery(id), ct);
                return File(_qr.GenerateQrCodeImage(shipment.QrCode), "image/png");
            }
            catch (NotFoundException) { return NotFound(); }
        }
    }
}
