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
        var restaurant = await _dbContext.Restaurants
            .FirstOrDefaultAsync(r => r.Id == request.RestaurantId, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Failed to mark order ready for pickup. Order {OrderId} was not found.", request.OrderId);
            throw new KeyNotFoundException($"Order with ID {request.OrderId} was not found.");
        }

        if (restaurant is null)
        {
            _logger.LogWarning("Failed to mark order ready for pickup. Restaurant {RestaurantId} was not found.", request.RestaurantId);
            throw new KeyNotFoundException($"Restaurant with ID {request.RestaurantId} was not found.");
        }

        // Transition domain state (This method inside Domain layer should also raise your OrderReadyForPickupDomainEvent)

        if (_dbContext is DbContext efDbContext)
        {

            using var transaction = await efDbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {

                // 1. Transition the core domain state
                order.TransitionToReadyForPickup();

                // 2. Create your integration contract matching pure MassTransit defaults
                var integrationEvent = OrderReadyForPickupIntegrationEvent.Create(
                    order.Id, order.RestaurantId, order.RestaurantName, DateTime.UtcNow,
                    restaurant.Address, restaurant.Latitude, restaurant.Longitude,
                    order.CustomerAddress, order.CustomerLatitude, order.CustomerLongitude);

                // 3. FIX: Publish BEFORE SaveChangesAsync.
                // Outbox intercepts this and adds internal event entities into the EF Change Tracker.
                await _publishEndpoint.Publish(integrationEvent, cancellationToken);

                // 4. Persist BOTH the order change and the outbox events to the DB in one atomic payload
                await _dbContext.SaveChangesAsync(cancellationToken);


                // 5. Commit safely
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
            // Fallback block to ensure testability if running unit tests with mock contexts
            order.TransitionToReadyForPickup();

            var integrationEvent = OrderReadyForPickupIntegrationEvent.Create(
                order.Id,
                order.RestaurantId,
                order.RestaurantName,
                DateTime.UtcNow, restaurant.Address, restaurant.Latitude, restaurant.Longitude,
                order.CustomerAddress, order.CustomerLatitude, order.CustomerLongitude
                );

            await _publishEndpoint.Publish(integrationEvent, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }


        _logger.LogInformation("Order {OrderId} has been marked as Ready For Pickup and logged successfully.", order.Id);

        return new MarkOrderReadyForPickupResponse(true);
    }
}