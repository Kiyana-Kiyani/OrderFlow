using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Features.Orders.CancelOrder;
using OrderFlow.Application.Features.Orders.Common;
using OrderFlow.Application.Features.Orders.GetMyOrders;
using OrderFlow.Application.Features.Orders.GetOrderById;
using OrderFlow.Application.Features.Orders.PlaceOrder;

namespace OrderFlow.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ISender _sender;

        public OrdersController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        [ProducesResponseType(typeof(PlaceOrderResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PlaceOrderResponse>> Place([FromBody] PlaceOrderCommand command, CancellationToken cancellationToken)
        {
            var response = await _sender.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = response.OrderId }, response);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrderDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var response = await _sender.Send(new GetOrderByIdQuery(id), cancellationToken);
            return Ok(response);
        }

        [HttpGet("my-orders")]
        [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetMyOrders(CancellationToken cancellationToken)
        {
            var response = await _sender.Send(new GetMyOrdersQuery(), cancellationToken);
            return Ok(response);
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new CancelOrderCommand(id), cancellationToken);
            return NoContent();
        }
    }
}