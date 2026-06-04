using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;

namespace OrderFlow.Application.Features.Couriers.CompleteDelivery
{

    public class CompleteDeliveryCommandHandler : IRequestHandler<CompleteDeliveryCommand, CompleteDeliveryResponse>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<CompleteDeliveryCommandHandler> _logger;

        public CompleteDeliveryCommandHandler(
            IApplicationDbContext dbContext,
            ICurrentUser currentUser,
            ILogger<CompleteDeliveryCommandHandler> logger)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<CompleteDeliveryResponse> Handle(CompleteDeliveryCommand request, CancellationToken cancellationToken)
        {
            var order = await _dbContext.CustomerOrders
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order is null)
            {
                _logger.LogWarning("Courier {CourierId} failed to complete order. Order {OrderId} not found.", _currentUser.UserId, request.OrderId);
                throw new KeyNotFoundException($"Order with ID {request.OrderId} was not found.");
            }

            order.TransitionToDelivered(_currentUser.UserId);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} marked as Delivered successfully by Courier {CourierId}.", order.Id, _currentUser.UserId);

            return new CompleteDeliveryResponse(true);
        }
    }
}