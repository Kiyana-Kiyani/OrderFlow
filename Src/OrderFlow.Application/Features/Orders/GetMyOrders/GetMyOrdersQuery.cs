using MediatR;
using OrderFlow.Application.Features.Orders.Common;

namespace OrderFlow.Application.Features.Orders.GetMyOrders
{
    public record GetMyOrdersQuery() : IRequest<IReadOnlyList<OrderSummaryDto>>;
}
