using OrderFlow.Application.Abstractions;
using StackExchange.Redis;

namespace OrderFlow.Infrastructure.Services
{
    public class CourierTrackerService : ICourierTrackerService
    {
        private readonly IDatabase _redisDb;

        public CourierTrackerService(IConnectionMultiplexer redisConnection)
        {
            _redisDb = redisConnection.GetDatabase();
        }

        public async Task TrackLocationAsync(Guid courierId, double latitude, double longitude)
        {
            string courierIdStr = courierId.ToString();
            string geoKey = "couriers:locations";
            string presenceKey = $"presence:courier:{courierIdStr}";

            await _redisDb.GeoAddAsync(geoKey, longitude, latitude, courierIdStr);
            await _redisDb.StringSetAsync(presenceKey, "Online", TimeSpan.FromMinutes(2));
        }
        public async Task RemoveFromLiveTrackingAsync(Guid courierId)
        {
            string courierIdStr = courierId.ToString();
            string geoKey = "couriers:locations";
            string presenceKey = $"presence:courier:{courierIdStr}";

            await _redisDb.GeoRemoveAsync(geoKey, courierIdStr);
            await _redisDb.KeyDeleteAsync(presenceKey);
        }

    }
}
