using MediatR;

namespace OrderFlow.Application.Features.MenuItems.MarkMenuItemAvailable
{
    public record MarkMenuItemAvailableCommand(
        Guid RestaurantId,
        Guid MenuItemId
    ) : IRequest;
}
