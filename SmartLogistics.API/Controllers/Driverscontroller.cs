using Microsoft.AspNetCore.Mvc;
using global::SmartLogistics.Application.Common.Models;
using global::SmartLogistics.Application.DTOs.Drivers;
using global::SmartLogistics.Application.DTOs.Shipments;
using global::SmartLogistics.Application.DTOs.Warehouses;
using global::SmartLogistics.Application.Features.Drivers.Commands;
using global::SmartLogistics.Application.Features.Drivers.Queries;
using global::SmartLogistics.Application.Features.Warehouses.Commands;
using global::SmartLogistics.Application.Features.Warehouses.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Application.Common.Models;
using SmartLogistics.Application.DTOs.Drivers;
using SmartLogistics.Application.DTOs.Warehouses;
using SmartLogistics.Application.Features.Drivers.Commands;
using SmartLogistics.Application.Features.Drivers.Queries;
using SmartLogistics.Application.Features.Warehouses.Commands;
using SmartLogistics.Application.Features.Warehouses.Queries;
using System.Security.Claims;
namespace SmartLogistics.API.Controllers
{
    

    /// <summary>
    /// Driver operations: GPS updates, task list, delivery completion, QR scanning.
    /// </summary>
    [ApiController]
    [Route("api/drivers")]
    [Authorize(Roles = "Driver,Admin")]
    [Produces("application/json")]
    public class DriversController : ControllerBase
    {
        private readonly IMediator _mediator;
        private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public DriversController(IMediator mediator) => _mediator = mediator;

        /// <summary>Update the authenticated driver's current GPS location.</summary>
        [HttpPost("me/location")]
        [Authorize(Roles = "Driver")]
        [ProducesResponseType(typeof(ApiResponse<DriverLocationDto>), 200)]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new UpdateDriverLocationCommand(CurrentUserId, request), ct);
            return Ok(ApiResponse<DriverLocationDto>.Ok(result));
        }

        /// <summary>Get all active (non-delivered) tasks for the authenticated driver.</summary>
        [HttpGet("me/tasks")]
        [Authorize(Roles = "Driver")]
        [ProducesResponseType(typeof(ApiResponse<List<DriverTaskDto>>), 200)]
        public async Task<IActionResult> GetMyTasks(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetDriverTasksQuery(CurrentUserId), ct);
            return Ok(ApiResponse<List<DriverTaskDto>>.Ok(result));
        }

        /// <summary>Scan shipment QR code to verify delivery. Driver only.</summary>
        [HttpPost("me/shipments/{shipmentId:guid}/scan-qr")]
        [Authorize(Roles = "Driver")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> ScanQr(Guid shipmentId, [FromBody] ScanQrRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new ScanQrCommand(CurrentUserId, shipmentId, request.QrCode), ct);
            return Ok(ApiResponse.Ok(result ? "QR verified successfully." : "QR verification failed."));
        }

        /// <summary>Mark a shipment as delivered after QR verification.</summary>
        [HttpPost("me/shipments/{shipmentId:guid}/complete")]
        [Authorize(Roles = "Driver")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> CompleteDelivery(Guid shipmentId,
            [FromQuery] string? notes, [FromQuery] string? photoUrl, CancellationToken ct)
        {
            var result = await _mediator.Send(new CompleteDeliveryCommand(CurrentUserId, shipmentId, notes, photoUrl), ct);
            return Ok(ApiResponse.Ok("Delivery completed successfully."));
        }

        /// <summary>Get tasks for a specific driver (Admin only).</summary>
        [HttpGet("{driverId:guid}/tasks")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDriverTasks(Guid driverId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetDriverTasksQuery(driverId), ct);
            return Ok(ApiResponse<List<DriverTaskDto>>.Ok(result));
        }
    }

    /// <summary>
    /// Warehouse CRUD and statistics. Admin only.
    /// </summary>
    [ApiController]
    [Route("api/warehouses")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    public class WarehousesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WarehousesController(IMediator mediator) => _mediator = mediator;

        /// <summary>Create a new warehouse.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), 201)]
        public async Task<IActionResult> Create([FromBody] CreateWarehouseRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new CreateWarehouseCommand(request), ct);
            return StatusCode(201, ApiResponse<WarehouseDto>.Created(result));
        }

        /// <summary>Update an existing warehouse.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WarehouseDto>), 200)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new UpdateWarehouseCommand(id, request), ct);
            return Ok(ApiResponse<WarehouseDto>.Ok(result));
        }

        /// <summary>Get shipment statistics and occupancy for a warehouse.</summary>
        [HttpGet("{id:guid}/statistics")]
        [ProducesResponseType(typeof(ApiResponse<WarehouseStatisticsDto>), 200)]
        public async Task<IActionResult> GetStatistics(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetWarehouseStatsQuery(id), ct);
            return Ok(ApiResponse<WarehouseStatisticsDto>.Ok(result));
        }
    }
}
