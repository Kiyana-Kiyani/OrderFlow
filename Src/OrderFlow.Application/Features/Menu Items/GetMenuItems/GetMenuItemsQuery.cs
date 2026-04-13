using MediatR;

namespace OrderFlow.Application.Features.Menu_Items.GetMenuItems
{
    public record GetMenuItemsQuery(Guid RestaurantId) : IRequest<IReadOnlyList<GetMenuItemsResponse>>;
}
