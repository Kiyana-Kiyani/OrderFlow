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
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RestaurantsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/<RestaurantsController>/
        [HttpGet]
        [ProducesResponseType(typeof(GetRestaurantsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetRestaurantsQuery(), cancellationToken);
            return Ok(result);
        }

        // GET: api/<RestaurantsController>/1
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetRestaurantByIdResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetRestaurantByIdQuery(id), cancellationToken);
            return Ok(response);
        }

        // DELETE api/<RestaurantsController>/5
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(CreateRestaurantResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RemoveRestaurantByIdCommand(id), cancellationToken);
            return Ok(result);
        }

        // POST api/<RestaurantsController>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(CreateRestaurantResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateResturant([FromBody] CreateRestaurantCommand createRestaurantCommand, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(createRestaurantCommand, cancellationToken);
            return Created(string.Empty, result);

        }

        //// PUT api/<RestaurantsController>/5
        //[HttpPatch("{restaurantId:guid}/menueitems/{menuItemId:guid}/name")]
        //public async Task<IActionResult> ChangeMneuItemName(Guid restaurantId, Guid menuItemId, [FromBody] , CancellationToken cancellationToken)
        //{


        //}

        [HttpPatch("{restaurantId:guid}/address")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ChangeRestaurantAddress(Guid restaurantId, [FromBody] string newAddress, CancellationToken cancellationToken)
        {
             await _mediator.Send(new ChangeRestaurantAddressCommand(restaurantId, newAddress), cancellationToken);
            return NoContent();
        }

        [HttpPatch("{restaurantId:guid}/activate")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ActivateRestaurant(Guid restaurantId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new ActivateRestaurantCommand(restaurantId), cancellationToken);
            return NoContent();

        }

        [HttpPatch("{restaurantId:guid}/dactivate")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeactivateRestaurant(Guid restaurantId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeactivateRestaurantCommand(restaurantId), cancellationToken);
            return NoContent();

        }

        [HttpPatch("{restaurantId:guid}/description")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ChangeRestaurantDescription(Guid restaurantId, [FromBody] string newDescription, CancellationToken cancellationToken)
        {
            await _mediator.Send(new ChangeRestaurantDescriptionCommand(restaurantId, newDescription), cancellationToken);
            return NoContent();
        }

        [HttpPatch("{restaurantId:guid}/name")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ChangeRestaurantName(Guid restaurantId, [FromBody] string newName, CancellationToken cancellationToken)
        {
            await _mediator.Send(new ChangeRestaurantNameCommand(restaurantId, newName), cancellationToken);
            return NoContent();
        }



    }
}
