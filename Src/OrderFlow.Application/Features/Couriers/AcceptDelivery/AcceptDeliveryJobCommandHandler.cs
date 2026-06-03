using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;

namespace OrderFlow.Application.Features.Couriers.AcceptDelivery;

public class AcceptDeliveryJobCommandHandler : IRequestHandler<AcceptDeliveryJobCommand, AcceptDeliveryJobResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AcceptDeliveryJobCommandHandler> _logger;

    public AcceptDeliveryJobCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        ILogger<AcceptDeliveryJobCommandHandler> logger)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<AcceptDeliveryJobResponse> Handle(AcceptDeliveryJobCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.CustomerOrders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Courier {CourierId} failed to accept delivery. Order {OrderId} not found.", _currentUser.UserId, request.OrderId);
            throw new KeyNotFoundException($"Order with ID {request.OrderId} was not found.");
        }
        // قانون دامین: بررسی آیدی پیک و تغییر وضعیت به Assign شده
        order.AssignCourier(_currentUser.UserId);

        try
        {
            // 👈 ذخیره تغییرات در دیتابیس
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // 🚀 مچ‌گیری هوشمندانه خطای همزمانی دیتابیس و تبدیل آن به خطای لایه Application
            _logger.LogWarning(ex, "Concurrency conflict occurred. Order {OrderId} was already claimed by another courier.", request.OrderId);

            throw new ConflictException("This order has already been claimed by another courier. Please refresh your available jobs list.");
        }

        _logger.LogInformation("Order {OrderId} successfully claimed by Courier {CourierId}.", order.Id, _currentUser.UserId);

        return new AcceptDeliveryJobResponse(true);
    }
}
