using MediatR;

namespace OrderFlow.Application.Features.Menu_Items.AddMenuItem
{
    public record AddMenuItemCommand(
        Guid RestaurantId,
        string Name,
        decimal Price,
        string? Description
    ) : IRequest<AddMenuItemResponse>;
}
