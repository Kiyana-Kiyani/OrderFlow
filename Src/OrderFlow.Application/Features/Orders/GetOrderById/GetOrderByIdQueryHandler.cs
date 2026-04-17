using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Features.Orders.Common;

namespace OrderFlow.Application.Features.Orders.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailsDto>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;

        public GetOrderByIdQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
        }

        public async Task<OrderDetailsDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _dbContext.CustomerOrders
                .Where(o => o.CustomerUserId == _currentUser.UserId && o.Id == request.OrderId)
                .Select(x => new OrderDetailsDto(
                    x.Id,
                    x.CustomerUserId,
                    x.RestaurantId,
                    x.RestaurantName,
                    x.Status,
                    x.TotalAmount,
                    x.CreatedAt,
                    x.OrderItems.Select(i => new OrderItemDto(
                        i.MenuItemId,
                        i.MenuItemName,
                        i.Quantity,
                        i.UnitPrice,
                        i.LineTotal
                       )).ToList()
                  ))
                 .FirstOrDefaultAsync(cancellationToken);

            if (order is null)
                throw new NotFoundException("Order", request.OrderId);

            return order;

        }
    }
}
