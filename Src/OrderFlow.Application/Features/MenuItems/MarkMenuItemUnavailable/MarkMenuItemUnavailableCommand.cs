using MediatR;

namespace OrderFlow.Application.Features.MenuItems.MarkMenuItemUnavailable;

public record MarkMenuItemUnavailableCommand(
    Guid RestaurantId,
    Guid MenuItemId
) : IRequest;

