using MediatR;

namespace OrderFlow.Application.Features.Orders.UpdatePaymentStatus
{
    public record UpdatePaymentStatusCommand(Guid OrderId, bool IsSucceeded) : IRequest;
}

