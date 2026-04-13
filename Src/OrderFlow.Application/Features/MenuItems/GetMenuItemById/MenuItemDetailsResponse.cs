namespace OrderFlow.Application.Features.MenuItems.GetMenuItemById
{
    public record MenuItemDetailsResponse(
        Guid Id,
        Guid RestaurantId,
        string Name,
        string? Description,
        decimal Price,
        bool IsAvailable
    );
}
