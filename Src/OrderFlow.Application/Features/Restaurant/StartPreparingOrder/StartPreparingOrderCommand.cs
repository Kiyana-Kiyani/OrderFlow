using MediatR;

namespace OrderFlow.Application.Features.Restaurant.StartPreparingOrder
{
    public record StartPreparingOrderCommand(Guid OrderId) : IRequest<StartPreparingOrderResponse>;
}



