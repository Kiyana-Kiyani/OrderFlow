using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;

namespace OrderFlow.Application.Features.Orders.CancelOrder
{
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, CancelOrderResponse>
    {

        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<CancelOrderCommandHandler> _logger;


        public CancelOrderCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser, ILogger<CancelOrderCommandHandler> logger)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<CancelOrderResponse> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _dbContext.CustomerOrders
                      .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order is null)
                throw new NotFoundException("Order", request.OrderId);

            if (order.CustomerUserId != _currentUser.UserId)
                throw new UnauthorizedAccessException("You are not allowed to edit this item.");

            order.Cancel();

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Order {OrderId} was cancelled by user {UserId}.",
               order.Id, _currentUser.UserId);
            return new CancelOrderResponse(order.Id, order.Status);
        }
    }
}
