using MediatR;

namespace OrderFlow.Application.Features.Orders.CancelOrder
{
    public record CancelOrderCommand(Guid OrderId) : IRequest<CancelOrderResponse>;
}
