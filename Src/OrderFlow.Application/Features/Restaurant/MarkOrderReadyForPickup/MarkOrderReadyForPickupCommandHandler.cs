using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Restaurant.MarkOrderReadyForPickup;

public class MarkOrderReadyForPickupCommandHandler : IRequestHandler<MarkOrderReadyForPickupCommand, MarkOrderReadyForPickupResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<MarkOrderReadyForPickupCommandHandler> _logger;

    public MarkOrderReadyForPickupCommandHandler(IApplicationDbContext dbContext, ILogger<MarkOrderReadyForPickupCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<MarkOrderReadyForPickupResponse> Handle(MarkOrderReadyForPickupCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.CustomerOrders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Failed to mark order ready for pickup. Order {OrderId} was not found.", request.OrderId);
            throw new KeyNotFoundException($"Order with ID {request.OrderId} was not found.");
        }

        // Transition domain state (This method inside Domain layer should also raise your OrderReadyForPickupDomainEvent)
        order.TransitionToReadyForPickup();

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} has been marked as Ready For Pickup and logged successfully.", order.Id);

        return new MarkOrderReadyForPickupResponse(true);
    }
}