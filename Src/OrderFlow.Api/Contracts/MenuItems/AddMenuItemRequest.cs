namespace OrderFlow.Api.Contracts.MenuItems
{
    public record AddMenuItemRequest(
        string Name,
        decimal Price,
        string? Description
    );
}
