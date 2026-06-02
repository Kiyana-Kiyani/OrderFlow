using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;

namespace OrderFlow.Application.Features.Couriers.ToggleAvailability;

public class ToggleCourierAvailabilityCommandHandler : IRequestHandler<ToggleCourierAvailabilityCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly ICourierTrackerService _courierTracker;

    public ToggleCourierAvailabilityCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        ICourierTrackerService courierTracker)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _courierTracker = courierTracker;
    }

    public async Task Handle(ToggleCourierAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var courier = await _dbContext.Couriers
            .FirstOrDefaultAsync(c => c.Id == _currentUser.UserId, cancellationToken);

        if (courier is null)
        {
            throw new NotFoundException("Courier Profile", _currentUser.UserId);
        }

        courier.ToggleAvailability();

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!courier.IsAvailable)
        {
            await _courierTracker.RemoveFromLiveTrackingAsync(courier.Id);
        }
    }
}