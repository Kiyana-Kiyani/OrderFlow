using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Features.Restaurant.ActivateRestaurant;
using OrderFlow.Application.Features.Restaurant.ChangeRestaurantAddress;
using OrderFlow.Application.Features.Restaurant.ChangeRestaurantDescription;
using OrderFlow.Application.Features.Restaurant.ChangeRestaurantName;
using OrderFlow.Application.Features.Restaurant.CreateRestaurant;
using OrderFlow.Application.Features.Restaurant.DeactivateRestaurant;
using OrderFlow.Application.Features.Restaurant.GetRestaurantById;
using OrderFlow.Application.Features.Restaurant.GetRestaurants;
using OrderFlow.Application.Features.Restaurant.MarkOrderReadyForPickup;
using OrderFlow.Application.Features.Restaurant.RemoveRestaurant;
using OrderFlow.Application.Features.Restaurant.StartPreparingOrder;


namespace OrderFlow.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RestaurantsController : ControllerBase
    {
        private readonly ISender _sender;
        public RestaurantsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GetRestaurantsResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GetRestaurantsResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var response = await _sender.Send(new GetRestaurantsQuery(), cancellationToken);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(GetRestaurantByIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetRestaurantByIdResponse>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var response = await _sender.Send(new GetRestaurantByIdQuery(id), cancellationToken);
            return Ok(response);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new RemoveRestaurantByIdCommand(id), cancellationToken);
            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CreateRestaurantResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CreateRestaurantResponse>> Create([FromBody] CreateRestaurantCommand createRestaurantCommand, CancellationToken cancellationToken)
        {
            var response = await _sender.Send(createRestaurantCommand, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = response.RestaurantId }, response);
        }

        [HttpPatch("{restaurantId:guid}/address")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeAddress([FromBody] ChangeRestaurantAddressCommand changeRestaurantAddress, CancellationToken cancellationToken)
        {
            await _sender.Send(changeRestaurantAddress, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{restaurantId:guid}/activate")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Activate(Guid restaurantId, CancellationToken cancellationToken)
        {
            await _sender.Send(new ActivateRestaurantCommand(restaurantId), cancellationToken);
            return NoContent();
        }

        [HttpPatch("{restaurantId:guid}/deactivate")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactivate(Guid restaurantId, CancellationToken cancellationToken)
        {
            await _sender.Send(new DeactivateRestaurantCommand(restaurantId), cancellationToken);
            return NoContent();
        }

        [HttpPatch("{restaurantId:guid}/description")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeDescription(Guid restaurantId, [FromBody] string newDescription, CancellationToken cancellationToken)
        {
            await _sender.Send(new ChangeRestaurantDescriptionCommand(restaurantId, newDescription), cancellationToken);
            return NoContent();
        }

        [HttpPatch("{restaurantId:guid}/name")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeName(Guid restaurantId, [FromBody] string newName, CancellationToken cancellationToken)
        {
            await _sender.Send(new ChangeRestaurantNameCommand(restaurantId, newName), cancellationToken);
            return NoContent();
        }

        [HttpPost("orders/{orderId:guid}/preparing")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MoveToPreparing([FromRoute] Guid orderId, CancellationToken cancellationToken)
        {
            await _sender.Send(new StartPreparingOrderCommand(orderId), cancellationToken);
            return NoContent();
        }

        [HttpPost("orders/{orderId:guid}/ready-for-pickup")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkReadyForPickup([FromBody] MarkOrderReadyForPickupCommand markOrderReadyForPickupCommand, CancellationToken cancellationToken)
        {
            await _sender.Send(markOrderReadyForPickupCommand, cancellationToken);
            return NoContent();
        }
    }
}
