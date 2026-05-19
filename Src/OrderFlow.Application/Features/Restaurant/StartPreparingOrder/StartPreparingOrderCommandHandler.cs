using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Restaurant.StartPreparingOrder;

public class StartPreparingOrderCommandHandler : IRequestHandler<StartPreparingOrderCommand, StartPreparingOrderResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<StartPreparingOrderCommandHandler> _logger;

    public StartPreparingOrderCommandHandler(IApplicationDbContext dbContext, ILogger<StartPreparingOrderCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<StartPreparingOrderResponse> Handle(StartPreparingOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.CustomerOrders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Failed to accept order. Order {OrderId} was not found.", request.OrderId);
            throw new KeyNotFoundException($"Order with ID {request.OrderId} was not found.");
        }

        // Uses your Domain method
        order.StartPreparing();

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} was successfully accepted by the restaurant.", order.Id);

        return new StartPreparingOrderResponse(true);
    }
}
