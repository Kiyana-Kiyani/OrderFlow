using MediatR;
using Microsoft.Extensions.Logging;

namespace OrderFlow.Application.Features.Orders.UpdateOrderStatus
{
    public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand>
    {
        private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

        public UpdateOrderStatusCommandHandler(ILogger<UpdateOrderStatusCommandHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling UpdateOrderStatusCommand for OrderId: {OrderId}", request.OrderId);
            return Task.CompletedTask;
        }
    }
}
