using MediatR;

namespace OrderFlow.Application.Features.MenuItems.ChangeMenuItemDescription
{
    public record ChangeMenuItemDescriptionCommand(
        Guid RestaurantId,
        Guid MenuItemId,
        string? Description
    ) : IRequest;
}
