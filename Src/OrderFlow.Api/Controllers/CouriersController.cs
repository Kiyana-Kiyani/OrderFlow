using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Features.Couriers.AcceptDelivery;
using OrderFlow.Application.Features.Couriers.CompleteDelivery;
using OrderFlow.Application.Features.Couriers.GetAvailableJobs;
using OrderFlow.Application.Features.Couriers.PickupOrder;
using OrderFlow.Application.Features.Couriers.UpdateLocation;

namespace OrderFlow.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize(Roles = "Courier,Admin")]
public class CouriersController : ControllerBase
{
    private readonly ISender _sender;

    public CouriersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("update-location")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationCommand command, CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("available-jobs")]
    [ProducesResponseType(typeof(IEnumerable<AvailableJobDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableJobs(CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetAvailableJobsQuery(), cancellationToken);
        return Ok(response);
    }

    [HttpPost("orders/{orderId:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptJob([FromRoute] Guid orderId, CancellationToken cancellationToken)
    {
        await _sender.Send(new AcceptDeliveryJobCommand(orderId), cancellationToken);
        return NoContent();
    }

    [HttpPost("orders/{orderId:guid}/pickup")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PickupOrder([FromRoute] Guid orderId, CancellationToken cancellationToken)
    {
        await _sender.Send(new PickupOrderCommand(orderId), cancellationToken);
        return NoContent();
    }

    [HttpPost("orders/{orderId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteDelivery([FromRoute] Guid orderId, CancellationToken cancellationToken)
    {
        await _sender.Send(new CompleteDeliveryCommand(orderId), cancellationToken);
        return NoContent();
    }
}