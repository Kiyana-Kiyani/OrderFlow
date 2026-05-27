namespace OrderFlow.Contracts
{
    public interface ICourierConnectionTracker
    {
        /// <summary>
        /// Looks up an active SignalR connection ID string string for a given Courier from Redis storage.
        /// </summary>
        Task<string?> GetConnectionIdAsync(Guid courierId);
    }
}
