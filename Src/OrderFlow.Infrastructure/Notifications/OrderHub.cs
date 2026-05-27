using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace OrderFlow.Infrastructure.Notifications
{
    // حتماً باید تایید هویت شده باشه تا به کلیم‌ها دسترسی داشته باشیم
    [Authorize]
    public class OrderHub : Hub
    {
        private readonly IDatabase _redisDb;

        // این متد توکارِ خود SignalR است و به محض اتصال کلاینت، خودکار اجرا میشه

        public OrderHub(IConnectionMultiplexer redisConnection)
        {
            _redisDb = redisConnection.GetDatabase();
        }


        public override async Task OnConnectedAsync()
        {

            // ۱. خواندن اطلاعات از توکن (JWT Claims)
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            var restaurantId = Context.User?.FindFirst("RestaurantId")?.Value; // کلیم اختصاصی شما

            // ۲. تصمیم‌گیری بر اساس نقش کاربر
            if (role == "Owner" && !string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(restaurantId))
            {
                // رستوران به گروه اختصاصی خودش وصل میشه
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Restaurant_{restaurantId}");
            }
            else if (role == "Courier")
            {

                string presenceKey = $"presence:{role?.ToLower()}:{userId}";

                // همه پیک‌ها عضو یک گروه عمومی میشن تا نوتیفیکیشن سفارش جدید رو بگیرن
                //await Groups.AddToGroupAsync(Context.ConnectionId, "Couriers");
                //await _redisDb.StringSetAsync(presenceKey, "Online", TimeSpan.FromMinutes(30));


                // یا اگر خواستید به یک پیک خاص پیام بدید، می‌تونید گروه اختصاصی براش بسازید:
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Courier_{userId}");
                await _redisDb.StringSetAsync(presenceKey, "Online", TimeSpan.FromMinutes(30));

            }
            else if (role == "Customer")
            {
                // مشتری به گروه اختصاصی خودش وصل میشه تا وضعیت سفارش خودش رو دنبال کنه
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{userId}");
            }
            else if (role == "Admin")
            {
                // ادمین‌ها به گروه کل سیستم وصل میشن
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }

            await base.OnConnectedAsync();
        }

        // Allows a restaurant dashboard client to join their specific group room
        // وقتی کاربر قطع متصل شد، خودش خودکار از گروه‌ها حذف میشه، اما متدش اینه:
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // خواندن مجدد اطلاعات توکن در نمونه جدید هاب هنگام قطع اتصال
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            // نیازی به حذف دستی از گروه‌ها نیست، SignalR خودش مدیریت میکنه
            if (role == "Courier" && !string.IsNullOrEmpty(userId))
            {
                string presenceKey = $"presence:{role.ToLower()}:{userId}";

                // حذف وضعیت آنلاین از ردیس
                await _redisDb.KeyDeleteAsync(presenceKey);
            }

            // مدیریت خروج از گروه‌ها خودکار توسط دات‌نت انجام می‌شود
            await base.OnDisconnectedAsync(exception);
        }
    }
}