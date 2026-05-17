using MediatR;

namespace OrderFlow.Application.Features.Orders.UpdateOrderStatus
{
    public record UpdateOrderStatusCommand(Guid OrderId) : IRequest
    {
    }
}
