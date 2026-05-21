using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Common.Exceptions;

namespace OrderFlow.Application.Features.Orders.UpdatePaymentStatus
{
    public class UpdatePaymentStatusCommandHandler : IRequestHandler<UpdatePaymentStatusCommand>
    {
        private readonly ILogger<UpdatePaymentStatusCommandHandler> _logger;
        private readonly IApplicationDbContext _dbContext;
        private readonly IOrderNotificationService _notificationService;

        public UpdatePaymentStatusCommandHandler(ILogger<UpdatePaymentStatusCommandHandler> logger, IApplicationDbContext dbContext,
            IOrderNotificationService notificationService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        public async Task Handle(UpdatePaymentStatusCommand request, CancellationToken cancellationToken)
        {
            var order = await _dbContext.CustomerOrders
               .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order is null)
                throw new NotFoundException("Order", request.OrderId);

            // Direct the state transition based on the incoming consumer outcome flag
            if (request.IsSucceeded)
            {
                order.MarkPaymentAsSucceeded();
                _logger.LogInformation("Payment confirmed for Order {OrderId}. Status is ready for kitchen review.", order.Id);

                // NEW: Push the live notification to the kitchen's active tablet/screen dashboard
                await _notificationService.NotifyRestaurantOfNewOrderAsync(
                    order.RestaurantId,
                    order.Id,
                    order.TotalAmount);
            }
            else
            {
                order.MarkPaymentAsFailed();
                _logger.LogWarning("Payment rejected for Order {OrderId}. System has auto-cancelled the routing context.", order.Id);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);


        }
    }
}
