using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Features.Orders.PlaceOrder
{
    public sealed record PlaceOrderResponse(
        Guid OrderId,
        OrderStatus Status,
        decimal TotalAmount,
        DateTime CreatedAtUtc
    );
}
