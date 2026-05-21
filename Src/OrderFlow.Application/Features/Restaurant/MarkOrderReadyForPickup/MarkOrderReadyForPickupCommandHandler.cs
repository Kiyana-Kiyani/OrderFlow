using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;
using OrderFlow.Contracts.IntegrationEvents;

namespace OrderFlow.Application.Features.Restaurant.MarkOrderReadyForPickup;

public class MarkOrderReadyForPickupCommandHandler : IRequestHandler<MarkOrderReadyForPickupCommand, MarkOrderReadyForPickupResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<MarkOrderReadyForPickupCommandHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public MarkOrderReadyForPickupCommandHandler(IApplicationDbContext dbContext, ILogger<MarkOrderReadyForPickupCommandHandler> logger,
        IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
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

        if (_dbContext is DbContext efDbContext)
        {

            using var transaction = await efDbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                order.TransitionToReadyForPickup();
                await _dbContext.SaveChangesAsync(cancellationToken);

                var integrationEvent = OrderReadyForPickupIntegrationEvent.Create(order.Id, order.RestaurantId, order.RestaurantName, DateTime.UtcNow);

                // Publish using a custom logistics key string
                await _publishEndpoint.Publish(integrationEvent, ctx => ctx.SetRoutingKey("order.ready"), cancellationToken);

                // 3. Save both to the database at the exact same millisecond
                await transaction.CommitAsync(cancellationToken);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while marking order {OrderId} as ready for pickup. Transaction is being rolled back.", order.Id);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }


        }
        else
        {

        }


        _logger.LogInformation("Order {OrderId} has been marked as Ready For Pickup and logged successfully.", order.Id);

        return new MarkOrderReadyForPickupResponse(true);
    }
}