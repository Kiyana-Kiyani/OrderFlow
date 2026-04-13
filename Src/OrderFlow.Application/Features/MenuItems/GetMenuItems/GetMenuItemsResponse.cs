namespace OrderFlow.Application.Features.MenuItems.GetMenuItems
{
    public record GetMenuItemsResponse
    (
     Guid Id,
     string Name,
     string? Description,
     decimal Price,
     bool IsAvailable
     );
}
