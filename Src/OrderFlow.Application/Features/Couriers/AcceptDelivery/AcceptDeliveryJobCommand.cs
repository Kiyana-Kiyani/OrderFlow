using MediatR;

namespace OrderFlow.Application.Features.Couriers.AcceptDelivery
{
    public record AcceptDeliveryJobCommand(Guid OrderId) : IRequest<AcceptDeliveryJobResponse>;
}