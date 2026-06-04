using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Common.Events;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Features.Couriers.EventHandlers
{
    public class CreateCourierProfileEventHandler : INotificationHandler<UserRoleAssignedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public CreateCourierProfileEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(UserRoleAssignedEvent notification, CancellationToken cancellationToken)
        {
            if (notification.Role != "Courier")
                return;

            var profileExists = await _dbContext.Couriers
                .AnyAsync(c => c.Id == notification.UserId, cancellationToken);

            if (profileExists)
                return;

            var newCourier = new Courier(
                id: notification.UserId,
                name: "New Courier",
                vehicleType: VehicleType.Bicycle
            );

            _dbContext.Couriers.Add(newCourier);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

