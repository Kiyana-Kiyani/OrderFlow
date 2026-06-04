using MediatR;

namespace OrderFlow.Application.Features.Couriers.CompleteDelivery
{
    public record CompleteDeliveryCommand(Guid OrderId) : IRequest<CompleteDeliveryResponse>;
}