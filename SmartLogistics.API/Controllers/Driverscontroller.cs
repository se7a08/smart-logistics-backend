using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediatR;
using SmartLogistics.Application.Common.Models;
using SmartLogistics.Application.DTOs.Drivers;
using SmartLogistics.Application.Features.Drivers.Commands;
using SmartLogistics.Application.Features.Drivers.Queries;
using SmartLogistics.Application.DTOs.Shipments;

namespace SmartLogistics.API.Controllers
{
    [ApiController]
    [Route("api/drivers")]
    [Authorize(Roles = "Driver,Admin")]
    public class DriversController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DriversController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Helper property to extract the Driver ID from the JWT token
        private Guid CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.Parse(userIdClaim!);
            }
        }

        // Updates the driver's real-time location (latitude/longitude)
        [HttpPost("me/location")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request, CancellationToken ct)
        {
            var command = new UpdateDriverLocationCommand(CurrentUserId, request);
            var result = await _mediator.Send(command, ct);

            return Ok(ApiResponse<DriverLocationDto>.Ok(result, "Location updated."));
        }

        // Retrieves the list of shipments assigned to the logged-in driver
        [HttpGet("me/tasks")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> GetMyTasks(CancellationToken ct)
        {
            var query = new GetDriverTasksQuery(CurrentUserId);
            var result = await _mediator.Send(query, ct);

            return Ok(ApiResponse<List<DriverTaskDto>>.Ok(result));
        }

        // Verifies a shipment delivery via QR code scanning
        [HttpPost("me/shipments/{shipmentId:guid}/scan-qr")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> ScanQr(Guid shipmentId, [FromBody] ScanQrRequest request, CancellationToken ct)
        {
            var command = new ScanQrCommand(CurrentUserId, shipmentId, request.QrCode);
            var isVerified = await _mediator.Send(command, ct);

            if (!isVerified)
            {
                return BadRequest(ApiResponse.Fail("QR Code verification failed. Invalid code."));
            }

            return Ok(ApiResponse.Ok("QR verified. You can now complete the delivery."));
        }

        // Finalizes the delivery process with optional notes and photos
        [HttpPost("me/shipments/{shipmentId:guid}/complete")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> CompleteDelivery(Guid shipmentId, [FromQuery] string? notes, [FromQuery] string? photoUrl, CancellationToken ct)
        {
            var command = new CompleteDeliveryCommand(CurrentUserId, shipmentId, notes, photoUrl);
            await _mediator.Send(command, ct);

            return Ok(ApiResponse.Ok("Shipment delivered successfully."));
        }

        // Admin only: Check tasks for any specific driver by their ID
        [HttpGet("{driverId:guid}/tasks")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDriverTasks(Guid driverId, CancellationToken ct)
        {
            var query = new GetDriverTasksQuery(driverId);
            var result = await _mediator.Send(query, ct);

            return Ok(ApiResponse<List<DriverTaskDto>>.Ok(result));
        }
    }
}