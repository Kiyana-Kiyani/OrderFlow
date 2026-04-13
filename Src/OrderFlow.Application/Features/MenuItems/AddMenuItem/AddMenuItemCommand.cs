using MediatR;

namespace OrderFlow.Application.Features.MenuItems.AddMenuItem
{
    public record AddMenuItemCommand(
        Guid RestaurantId,
        string Name,
        decimal Price,
        string? Description
    ) : IRequest<AddMenuItemResponse>;
}
