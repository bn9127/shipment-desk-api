using Microsoft.AspNetCore.Mvc;
using ShipmentDesk.Api.Contracts;
using ShipmentDesk.Api.Domain;
using ShipmentDesk.Api.Security;
using ShipmentDesk.Api.Services;

namespace ShipmentDesk.Api.Controllers;

[ApiController]
[Route("api/shipments")]
[ServiceFilter(typeof(ApiKeyFilter))]
public sealed class ShipmentsController : ControllerBase
{
    private readonly ShipmentService _shipments;

    public ShipmentsController(ShipmentService shipments)
    {
        _shipments = shipments;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShipmentResponse>>> List([FromQuery] string? status, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _shipments.ListAsync(status, cancellationToken));
        }
        catch (ShipmentRuleException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShipmentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var shipment = await _shipments.GetAsync(id, cancellationToken);
        return shipment is null ? NotFound(new { error = $"Shipment '{id}' was not found." }) : Ok(shipment);
    }

    [HttpPost]
    public async Task<ActionResult<ShipmentResponse>> Create(CreateShipmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _shipments.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ShipmentRuleException exception)
        {
            return Conflict(new { error = exception.Message });
        }
    }

    [HttpPost("{id:guid}/milestones")]
    public async Task<ActionResult<ShipmentResponse>> AddMilestone(Guid id, AddMilestoneRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _shipments.AddMilestoneAsync(id, request, cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { error = exception.Message });
        }
        catch (ShipmentRuleException exception)
        {
            return Conflict(new { error = exception.Message });
        }
    }
}
