using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Features.Orders.CancelOrder
{
    public record CancelOrderResponse(
        Guid OrderId,
        OrderStatus Status
    );
}
