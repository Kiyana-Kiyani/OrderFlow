using MediatR;

namespace OrderFlow.Application.Features.Menu_Items.ChangeMenuItemPrice
{
    public record ChangeMenuItemPriceCommand(
        Guid RestaurantId,
        Guid MenuItemId,
        decimal Price
    ) : IRequest;
}
