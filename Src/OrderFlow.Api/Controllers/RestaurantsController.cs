using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Features.Resturant.ActivateRestaurant;
using OrderFlow.Application.Features.Resturant.ChangeRestaurantAddress;
using OrderFlow.Application.Features.Resturant.ChangeRestaurantDescription;
using OrderFlow.Application.Features.Resturant.ChangeRestaurantName;
using OrderFlow.Application.Features.Resturant.CreateRestaurant;
using OrderFlow.Application.Features.Resturant.DeactivateRestaurant;
using OrderFlow.Application.Features.Resturant.GetResturantById;
using OrderFlow.Application.Features.Resturant.GetResturants;
using OrderFlow.Application.Features.Resturant.RemoveResturant;


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
        public async Task<ActionResult<CreateRestaurantResponse>> CreateRestaurant([FromBody] CreateRestaurantCommand createRestaurantCommand, CancellationToken cancellationToken)
        {
            var response = await _sender.Send(createRestaurantCommand, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = response.RestaurantId }, response);
        }

        [HttpPatch("{restaurantId:guid}/address")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeRestaurantAddress(Guid restaurantId, [FromBody] string newAddress, CancellationToken cancellationToken)
        {
            await _sender.Send(new ChangeRestaurantAddressCommand(restaurantId, newAddress), cancellationToken);
            return NoContent();
        }

        [HttpPatch("{restaurantId:guid}/activate")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateRestaurant(Guid restaurantId, CancellationToken cancellationToken)
        {
            await _sender.Send(new ActivateRestaurantCommand(restaurantId), cancellationToken);
            return NoContent();
        }

        [HttpPatch("{restaurantId:guid}/deactivate")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateRestaurant(Guid restaurantId, CancellationToken cancellationToken)
        {
            await _sender.Send(new DeactivateRestaurantCommand(restaurantId), cancellationToken);
            return NoContent();
        }

        [HttpPatch("{restaurantId:guid}/description")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeRestaurantDescription(Guid restaurantId, [FromBody] string newDescription, CancellationToken cancellationToken)
        {
            await _sender.Send(new ChangeRestaurantDescriptionCommand(restaurantId, newDescription), cancellationToken);
            return NoContent();
        }

        [HttpPatch("{restaurantId:guid}/name")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeRestaurantName(Guid restaurantId, [FromBody] string newName, CancellationToken cancellationToken)
        {
            await _sender.Send(new ChangeRestaurantNameCommand(restaurantId, newName), cancellationToken);
            return NoContent();
        }
    }
}
