using MediatR;
using OrderFlow.Application.Features.Orders.Common;

namespace OrderFlow.Application.Features.Orders.GetOrderById
{
    public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDetailsDto>;
}
