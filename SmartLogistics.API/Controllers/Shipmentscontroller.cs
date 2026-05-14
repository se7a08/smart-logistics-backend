using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using System.Security.Claims;
using SmartLogistics.Application.Common.Models;
using SmartLogistics.Application.DTOs.Shipments;
using SmartLogistics.Application.Features.Shipments.Commands;
using SmartLogistics.Application.Features.Shipments.Queries;
using SmartLogistics.Domain.Interfaces;

namespace SmartLogistics.API.Controllers
{
    [ApiController]
    [Route("api/shipments")]
    [Authorize]
    public class ShipmentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ShipmentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Extracts the currently logged-in User ID
        private Guid CurrentUserId
        {
            get
            {
                var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.Parse(id!);
            }
        }

        // Creates a new shipment - restricted to Admin
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateShipmentRequest request, CancellationToken ct)
        {
            var command = new CreateShipmentCommand(request);
            var result = await _mediator.Send(command, ct);

            var response = ApiResponse<ShipmentDto>.Created(result, "Shipment created successfully.");
            return StatusCode(201, response);
        }

        // Lists all shipments with filters and pagination - Admin only
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] QueryParameters paging, [FromQuery] string? status, CancellationToken ct)
        {
            var query = new GetAllShipmentsQuery(paging, status);
            var result = await _mediator.Send(query, ct);

            return Ok(ApiResponse<PaginatedList<ShipmentDto>>.Ok(result));
        }

        // Gets specific shipment details by ID
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var query = new GetShipmentByIdQuery(id);
            var result = await _mediator.Send(query, ct);

            return Ok(ApiResponse<ShipmentDto>.Ok(result));
        }

        // Returns the full tracking history of a shipment
        [HttpGet("{id:guid}/history")]
        public async Task<IActionResult> GetHistory(Guid id, CancellationToken ct)
        {
            var query = new GetShipmentHistoryQuery(id);
            var result = await _mediator.Send(query, ct);

            return Ok(ApiResponse<List<ShipmentStatusHistoryDto>>.Ok(result));
        }

        // Updates shipment status (e.g., PickedUp, InTransit)
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateShipmentStatusRequest request, CancellationToken ct)
        {
            var command = new UpdateShipmentStatusCommand(id, CurrentUserId, request);
            var result = await _mediator.Send(command, ct);

            return Ok(ApiResponse<ShipmentDto>.Ok(result, "Shipment status has been updated."));
        }

        // Assigns a specific driver to a shipment - Admin only
        [HttpPost("{id:guid}/assign-driver")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignDriver(Guid id, [FromBody] AssignDriverRequest request, CancellationToken ct)
        {
            var command = new AssignDriverCommand(id, request.DriverId);
            var result = await _mediator.Send(command, ct);

            return Ok(ApiResponse<ShipmentDto>.Ok(result, "Driver assigned to shipment."));
        }

        // Generates and returns a QR code image as a PNG file
        [HttpGet("{id:guid}/qr-image")]
        public async Task<IActionResult> GetQrImage(Guid id, CancellationToken ct)
        {
            var query = new GetShipmentByIdQuery(id);
            var shipment = await _mediator.Send(query, ct);

            // Accessing the QR service via Dependency Injection
            var qrService = HttpContext.RequestServices.GetRequiredService<IQrCodeService>();

            var imageBytes = qrService.GenerateQrCodeImage(shipment.QrCode);

            var fileName = $"shipment-{shipment.TrackingNumber}.png";
            return File(imageBytes, "image/png", fileName);
        }
    }
}