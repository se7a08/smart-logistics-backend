using Microsoft.AspNetCore.Mvc;
using global::SmartLogistics.Application.Common.Models;
using global::SmartLogistics.Application.DTOs.Shipments;
using global::SmartLogistics.Application.Features.Shipments.Commands;
using global::SmartLogistics.Application.Features.Shipments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Application.Common.Models;
using SmartLogistics.Application.DTOs.Shipments;
using SmartLogistics.Application.Features.Shipments.Commands;
using SmartLogistics.Application.Features.Shipments.Queries;
using System.Security.Claims;

namespace SmartLogistics.API.Controllers
{

    /// <summary>
    /// Full shipment lifecycle management: create, track, assign drivers, update status.
    /// </summary>
    [ApiController]
    [Route("api/shipments")]
    [Authorize]
    [Produces("application/json")]
    public class ShipmentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public ShipmentsController(IMediator mediator) => _mediator = mediator;

        /// <summary>Create a new shipment. Admin only.</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), 201)]
        public async Task<IActionResult> Create([FromBody] CreateShipmentRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new CreateShipmentCommand(request), ct);
            return StatusCode(201, ApiResponse<ShipmentDto>.Created(result));
        }

        /// <summary>Get all shipments with pagination and optional status filter. Admin only.</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<ShipmentDto>>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] QueryParameters paging, [FromQuery] string? status, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllShipmentsQuery(paging, status), ct);
            return Ok(ApiResponse<PaginatedList<ShipmentDto>>.Ok(result));
        }

        /// <summary>Get a single shipment by ID.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetShipmentByIdQuery(id), ct);
            return Ok(ApiResponse<ShipmentDto>.Ok(result));
        }

        /// <summary>Get the full status history for a shipment.</summary>
        [HttpGet("{id:guid}/history")]
        [ProducesResponseType(typeof(ApiResponse<List<ShipmentStatusHistoryDto>>), 200)]
        public async Task<IActionResult> GetHistory(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetShipmentHistoryQuery(id), ct);
            return Ok(ApiResponse<List<ShipmentStatusHistoryDto>>.Ok(result));
        }

        /// <summary>Update the status of a shipment. Driver or Admin.</summary>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), 200)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateShipmentStatusRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new UpdateShipmentStatusCommand(id, CurrentUserId, request), ct);
            return Ok(ApiResponse<ShipmentDto>.Ok(result, "Status updated successfully."));
        }

        /// <summary>Assign a driver to a pending shipment. Admin only.</summary>
        [HttpPost("{id:guid}/assign-driver")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<ShipmentDto>), 200)]
        public async Task<IActionResult> AssignDriver(Guid id, [FromBody] AssignDriverRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new AssignDriverCommand(id, request.DriverId), ct);
            return Ok(ApiResponse<ShipmentDto>.Ok(result, "Driver assigned successfully."));
        }

        /// <summary>Get the QR code image for a shipment as PNG. Admin or assigned Driver.</summary>
        [HttpGet("{id:guid}/qr-image")]
        [Produces("image/png")]
        public async Task<IActionResult> GetQrImage(Guid id, CancellationToken ct)
        {
            var shipment = await _mediator.Send(new GetShipmentByIdQuery(id), ct);
            var qrService = HttpContext.RequestServices
                .GetRequiredService<SmartLogistics.Domain.Interfaces.IQrCodeService>();
            var imageBytes = qrService.GenerateQrCodeImage(shipment.QrCode);
            return File(imageBytes, "image/png", $"shipment-{shipment.TrackingNumber}.png");
        }
    }
}
