using MediatR;

namespace OrderFlow.Application.Features.Menu_Items.GetMenuItemById
{
    public record GetMenuItemByIdQuery(
        Guid RestaurantId,
        Guid MenuItemId
    ) : IRequest<MenuItemDetailsResponse>;
}
