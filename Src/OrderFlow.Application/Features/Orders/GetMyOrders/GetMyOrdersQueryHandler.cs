using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Features.Orders.Common;

namespace OrderFlow.Application.Features.Orders.GetMyOrders
{

    public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, IReadOnlyList<OrderSummaryDto>>
    {

        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;

        public GetMyOrdersQueryHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyList<OrderSummaryDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = await _dbContext.CustomerOrders
                .Where(o => o.CustomerUserId == _currentUser.UserId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(x => new
                OrderSummaryDto(
                    x.Id,
                    x.RestaurantId,
                    x.RestaurantName,
                    x.Status,
                    x.TotalAmount,
                    x.CreatedAt
                )).ToListAsync(cancellationToken);
            return orders;
        }
    }
}
