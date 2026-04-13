using MediatR;

namespace OrderFlow.Application.Features.MenuItems.ChangeMenuItemPrice
{
    public record ChangeMenuItemPriceCommand(
        Guid RestaurantId,
        Guid MenuItemId,
        decimal Price
    ) : IRequest;
}
