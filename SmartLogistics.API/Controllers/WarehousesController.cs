using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using SmartLogistics.Application.Common.Models;
using SmartLogistics.Application.DTOs.Warehouses;
using SmartLogistics.Application.Features.Warehouses.Commands;
using SmartLogistics.Application.Features.Warehouses.Queries;

namespace SmartLogistics.API.Controllers
{
    [ApiController]
    [Route("api/warehouses")]
    [Authorize(Roles = "Admin")]
    public class WarehousesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WarehousesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Creates a new warehouse center
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWarehouseRequest request, CancellationToken ct)
        {
            var command = new CreateWarehouseCommand(request);
            var result = await _mediator.Send(command, ct);

            var response = ApiResponse<WarehouseDto>.Created(result, "Warehouse created successfully.");
            return StatusCode(201, response);
        }

        // Updates existing warehouse information (capacity, manager, location, etc.)
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseRequest request, CancellationToken ct)
        {
            var command = new UpdateWarehouseCommand(id, request);
            var result = await _mediator.Send(command, ct);

            var response = ApiResponse<WarehouseDto>.Ok(result, "Warehouse updated successfully.");
            return Ok(response);
        }

        // Retrieves statistics and occupancy levels for a specific warehouse
        [HttpGet("{id:guid}/statistics")]
        public async Task<IActionResult> GetStatistics(Guid id, CancellationToken ct)
        {
            var query = new GetWarehouseStatsQuery(id);
            var result = await _mediator.Send(query, ct);

            return Ok(ApiResponse<WarehouseStatisticsDto>.Ok(result));
        }
    }
}