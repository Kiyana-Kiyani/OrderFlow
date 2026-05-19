using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;

namespace OrderFlow.Application.Features.Couriers.PickupOrder;

public class PickupOrderCommandHandler : IRequestHandler<PickupOrderCommand, PickupOrderResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<PickupOrderCommandHandler> _logger;

    public PickupOrderCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        ILogger<PickupOrderCommandHandler> logger)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<PickupOrderResponse> Handle(PickupOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.CustomerOrders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Courier {CourierId} failed to pick up order. Order {OrderId} not found.", _currentUser.UserId, request.OrderId);
            throw new KeyNotFoundException($"Order with ID {request.OrderId} was not found.");
        }

        // Domain rule: Confirms the picking courier is the assigned driver, then mutates state to OutForDelivery
        order.TransitionToOutForDelivery(_currentUser.UserId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} picked up by Courier {CourierId}. Status updated to OutForDelivery.", order.Id, _currentUser.UserId);

        return new PickupOrderResponse(true);
    }
}