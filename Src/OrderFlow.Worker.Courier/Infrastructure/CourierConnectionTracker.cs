using Microsoft.Extensions.Caching.Distributed;
using OrderFlow.Contracts;

namespace OrderFlow.Worker.Courier.Infrastructure
{
    internal class CourierConnectionTracker : ICourierConnectionTracker
    {
        private readonly IDistributedCache _cache;

        public CourierConnectionTracker(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<string?> GetConnectionIdAsync(Guid courierId)
        {
            // Generates the deterministic lookup string key path: "OrderFlow:courier:connection:GUID"
            string cacheKey = $"courier:connection:{courierId}";

            // Query the global Redis key-value storage cluster pool directly
            return await _cache.GetStringAsync(cacheKey);
        }
    }
}
