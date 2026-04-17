namespace OrderFlow.Application.Features.Orders.PlaceOrder
{
    public sealed record PlaceOrderItemCommand(
        Guid MenuItemId,
        int Quantity
    );
}
