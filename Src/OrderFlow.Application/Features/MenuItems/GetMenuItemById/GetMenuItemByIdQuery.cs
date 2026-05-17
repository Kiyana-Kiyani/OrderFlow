using MediatR;

namespace OrderFlow.Application.Features.MenuItems.GetMenuItemById
{
    public record GetMenuItemByIdQuery(
        Guid RestaurantId,
        Guid MenuItemId
    ) : IRequest<MenuItemDetailsResponse>;
}
