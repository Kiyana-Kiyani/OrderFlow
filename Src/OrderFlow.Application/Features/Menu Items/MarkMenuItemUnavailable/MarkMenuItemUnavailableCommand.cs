using MediatR;

namespace OrderFlow.Application.Features.Menu_Items.MarkMenuItemUnavailable;

public record MarkMenuItemUnavailableCommand(
    Guid RestaurantId,
    Guid MenuItemId
) : IRequest;

