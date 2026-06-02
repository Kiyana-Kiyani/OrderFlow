using OrderFlow.Application.Abstractions;
using StackExchange.Redis;

namespace OrderFlow.Infrastructure
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

            // کدهای بومی ردیس جئو و استرینگ کاملا در زیرساخت کپسوله‌سازی میشن
            await _redisDb.GeoAddAsync(geoKey, longitude, latitude, courierIdStr);
            await _redisDb.StringSetAsync(presenceKey, "Online", TimeSpan.FromMinutes(2));
        }
        public async Task RemoveFromLiveTrackingAsync(Guid courierId)
        {
            string courierIdStr = courierId.ToString();
            string geoKey = "couriers:locations";
            string presenceKey = $"presence:courier:{courierIdStr}";

            // حذف مشخصات جغرافیایی از روی نقشه ریدیس
            await _redisDb.GeoRemoveAsync(geoKey, courierIdStr);
            // حذف کلید وضعیت آنلاین بودن پیک
            await _redisDb.KeyDeleteAsync(presenceKey);
        }

    }
}
