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

        public UpdatePaymentStatusCommandHandler(ILogger<UpdatePaymentStatusCommandHandler> logger, IApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
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
