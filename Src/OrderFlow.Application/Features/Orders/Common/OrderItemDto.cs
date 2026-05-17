namespace OrderFlow.Application.Features.Orders.Common
{
    public sealed record OrderItemDto(
        Guid MenuItemId,
        string MenuItemName,
        int Quantity,
        decimal UnitPrice,
        decimal LineTotal
    );
}
