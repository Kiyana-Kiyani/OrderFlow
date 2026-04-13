namespace OrderFlow.Application.Features.Menu_Items.GetMenuItemById
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
