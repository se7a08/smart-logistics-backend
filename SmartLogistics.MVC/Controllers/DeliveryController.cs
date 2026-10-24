using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Application.Common.Exceptions;
using SmartLogistics.Application.DTOs.Shipments;
using SmartLogistics.Application.Features.Drivers.Commands;
using SmartLogistics.Application.Features.Shipments.Commands;
using SmartLogistics.Application.Features.Shipments.Queries;
using SmartLogistics.Domain.Enums;

namespace SmartLogistics.MVC.Controllers
{
    [Authorize(Roles = "Admin,WarehouseManager,Driver")]
    public class DeliveryController : Controller
    {
        private readonly IMediator _mediator;
        public DeliveryController(IMediator mediator) => _mediator = mediator;

        // Shipments In transit that are waiting for delivery confirmation
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var all = await _mediator.Send(new GetAllShipmentsQuery(
                new SmartLogistics.Application.Common.Models.QueryParameters { PageSize = 100 }, "InTransit"), ct);
            return View(all.Items);
        }

        [HttpGet]
        public async Task<IActionResult> Scan(Guid id, CancellationToken ct)
        {
            try { return View(await _mediator.Send(new GetShipmentByIdQuery(id), ct)); }
            catch (NotFoundException) { return NotFound(); }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(Guid id, string qrCode, string? notes, CancellationToken ct)
        {
            try
            {
                var shipment = await _mediator.Send(new GetShipmentByIdQuery(id), ct);
                if (shipment.Status != "InTransit" || shipment.DriverId is null)
                    throw new BusinessRuleException("Shipment must be InTransit with an assigned driver.");

                await _mediator.Send(new ScanQrCommand(shipment.DriverId.Value, id, qrCode.Trim()), ct);

                var userId = Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var g) ? g : Guid.Empty;
                await _mediator.Send(new UpdateShipmentStatusCommand(id, userId,
                    new UpdateShipmentStatusRequest(ShipmentStatus.Delivered, notes ?? "Delivered, QR verified", null, null)), ct);

                TempData["Success"] = $"Shipment {shipment.TrackingNumber} delivered and verified.";
                return RedirectToAction("Details", "Shipments", new { id });
            }
            catch (Exception ex) when (ex is NotFoundException or BusinessRuleException)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Scan), new { id });
            }
        }
    }
}
