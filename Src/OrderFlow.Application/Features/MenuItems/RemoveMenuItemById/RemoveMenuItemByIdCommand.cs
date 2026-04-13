using MediatR;

namespace OrderFlow.Application.Features.MenuItems.RemoveMenuItemById;

public record RemoveMenuItemByIdCommand(
    Guid RestaurantId,
    Guid MenuItemId
) : IRequest;