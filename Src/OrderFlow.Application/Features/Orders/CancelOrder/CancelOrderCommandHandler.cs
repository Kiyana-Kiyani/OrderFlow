using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Application.Features.Orders.CancelOrder
{
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, CancelOrderResponse>
    {

        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;

        public CancelOrderCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
        }

        public async Task<CancelOrderResponse> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _dbContext.CustomerOrders
                      .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order is null)
                throw new Exception("Order not found.");

            if (order.CustomerUserId != _currentUser.UserId)
                throw new Exception("You are not allowed to cancel this order.");

            order.Cancel();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CancelOrderResponse(order.Id, order.Status);
        }
    }
}
