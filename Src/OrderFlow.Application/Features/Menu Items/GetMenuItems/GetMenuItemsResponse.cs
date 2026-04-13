namespace OrderFlow.Application.Features.Menu_Items.GetMenuItems
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
