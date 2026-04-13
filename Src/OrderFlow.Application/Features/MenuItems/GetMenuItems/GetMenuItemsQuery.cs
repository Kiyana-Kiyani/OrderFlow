using MediatR;

namespace OrderFlow.Application.Features.MenuItems.GetMenuItems
{
    public record GetMenuItemsQuery(Guid RestaurantId) : IRequest<IReadOnlyList<GetMenuItemsResponse>>;
}
