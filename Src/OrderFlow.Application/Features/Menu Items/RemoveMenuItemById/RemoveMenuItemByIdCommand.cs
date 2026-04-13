using MediatR;

namespace OrderFlow.Application.Features.Menu_Items.RemoveMenuItemById;

public record RemoveMenuItemByIdCommand(
    Guid RestaurantId,
    Guid MenuItemId
) : IRequest;