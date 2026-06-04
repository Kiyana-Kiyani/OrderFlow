namespace OrderFlow.Application.Abstractions
{
    public interface ICourierTrackerService
    {
        Task TrackLocationAsync(Guid courierId, double latitude, double longitude);
        Task RemoveFromLiveTrackingAsync(Guid courierId);
    }
}
