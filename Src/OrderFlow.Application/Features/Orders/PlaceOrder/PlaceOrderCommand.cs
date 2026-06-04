using MediatR;

namespace OrderFlow.Application.Features.Orders.PlaceOrder
{
    public sealed record PlaceOrderCommand(
        Guid RestaurantId,
        List<PlaceOrderItemCommand> Items,
        string CustomerAddress,
        double CustomerLatitude,
        double CustomerLongitude
    ) : IRequest<PlaceOrderResponse>;
}
