using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Contracts.MenuItems;
using OrderFlow.Application.Features.MenuItems.AddMenuItem;
using OrderFlow.Application.Features.MenuItems.ChangeMenuItemDescription;
using OrderFlow.Application.Features.MenuItems.ChangeMenuItemPrice;
using OrderFlow.Application.Features.MenuItems.GetMenuItemById;
using OrderFlow.Application.Features.MenuItems.GetMenuItems;
using OrderFlow.Application.Features.MenuItems.MarkMenuItemAvailable;
using OrderFlow.Application.Features.MenuItems.MarkMenuItemUnavailable;
using OrderFlow.Application.Features.MenuItems.RemoveMenuItemById;

namespace OrderFlow.Api.Controllers
{
    [ApiController]
    [Route("api/restaurants/{restaurantId:guid}/menu-items")]
    public class MenuItemsController : ControllerBase
    {
        private readonly ISender _sender;

        public MenuItemsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<GetMenuItemsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMenuItems([FromRoute] Guid restaurantId, CancellationToken cancellationToken)
        {
            var response = await _sender.Send(new GetMenuItemsQuery(restaurantId), cancellationToken);
            return Ok(response);
        }

        [HttpGet("{menuItemId:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(MenuItemDetailsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMenuItemById([FromRoute] Guid restaurantId, [FromRoute] Guid menuItemId, CancellationToken cancellationToken)
        {
            var response = await _sender.Send(new GetMenuItemByIdQuery(restaurantId, menuItemId), cancellationToken);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles ="Admin,Owner")]
        [ProducesResponseType(typeof(MenuItemDetailsResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddMenuItem([FromRoute] Guid restaurantId, [FromBody] AddMenuItemRequest request, CancellationToken cancellationToken)
        {
            var command = new AddMenuItemCommand(restaurantId, request.Name, request.Price, request.Description);

            var response = await _sender.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetMenuItemById), new { restaurantId = restaurantId, menuItemId = response.MenuItemId }, response);
        }

        [HttpPatch("{menuItemId:guid}/description")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeMenuItemDescription([FromRoute] Guid restaurantId, [FromRoute] Guid menuItemId, [FromBody] string? description, CancellationToken cancellationToken)
        {
            var command = new ChangeMenuItemDescriptionCommand(restaurantId, menuItemId, description);
            await _sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{menuItemId:guid}/price")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeMenuItemPrice([FromRoute] Guid restaurantId, [FromRoute] Guid menuItemId, [FromBody] decimal price, CancellationToken cancellationToken)
        {
            var command = new ChangeMenuItemPriceCommand(restaurantId, menuItemId, price);
            await _sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{menuItemId:guid}/available")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MarkMenuItemAvailable([FromRoute] Guid restaurantId, [FromRoute] Guid menuItemId, CancellationToken cancellationToken)
        {
            var command = new MarkMenuItemAvailableCommand(restaurantId, menuItemId);
            await _sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{menuItemId:guid}/unavailable")]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MarkMenuItemUnavailable([FromRoute] Guid restaurantId, [FromRoute] Guid menuItemId, CancellationToken cancellationToken)
        {
            var command = new MarkMenuItemUnavailableCommand(restaurantId, menuItemId);
            await _sender.Send(command, cancellationToken);
            return NoContent();
        }


        [HttpDelete]
        [Authorize(Roles = "Admin,Owner")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteMenuItem([FromRoute] Guid restaurantId, [FromRoute] Guid menuItemId, CancellationToken cancellationToken)
        {
            var command = new RemoveMenuItemByIdCommand(restaurantId, menuItemId);
            await _sender.Send(command, cancellationToken);
            return NoContent();
        }
    }
}