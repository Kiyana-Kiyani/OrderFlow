using MediatR;

namespace OrderFlow.Application.Features.Menu_Items.ChangeMenuItemDescription
{
    public record ChangeMenuItemDescriptionCommand(
        Guid RestaurantId,
        Guid MenuItemId,
        string? Description
    ) : IRequest;
}
