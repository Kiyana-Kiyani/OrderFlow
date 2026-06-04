using MediatR;

namespace OrderFlow.Application.Features.Couriers.PickupOrder
{
    public record PickupOrderCommand(Guid OrderId) : IRequest<PickupOrderResponse>;
}