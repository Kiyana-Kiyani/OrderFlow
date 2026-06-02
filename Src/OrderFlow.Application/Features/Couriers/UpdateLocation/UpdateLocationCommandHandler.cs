using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;

namespace OrderFlow.Application.Features.Couriers.UpdateLocation
{
    public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, UpdateLocationResponse>
    {
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<UpdateLocationCommandHandler> _logger;
        private readonly ICourierTrackerService _courierTracker;

        public UpdateLocationCommandHandler(
            ICourierTrackerService courierTracker,
            ICurrentUser currentUser,
            ILogger<UpdateLocationCommandHandler> logger)
        {
            _currentUser = currentUser;
            _logger = logger;
            _courierTracker = courierTracker;
        }

        public async Task<UpdateLocationResponse> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
        {
            var courierId = _currentUser.UserId;

            if (courierId == Guid.Empty)
            {
                _logger.LogWarning("An unauthenticated or invalid user attempted to update courier location.");
                throw new UnauthorizedAccessException("Courier identity is invalid.");
            }

            await _courierTracker.TrackLocationAsync(courierId, request.Latitude, request.Longitude);

            _logger.LogInformation("Courier {CourierId} successfully updated location to (Lat: {Latitude}, Lng: {Longitude}) and marked as Online.",
                courierId, request.Latitude, request.Longitude);

            return new UpdateLocationResponse(true);
        }
    }
}