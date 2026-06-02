using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrderFlow.Contracts.IntegrationEvents;
using OrderFlow.Worker.Courier.Tests.Fixtures;
using StackExchange.Redis;

namespace OrderFlow.Worker.Courier.Tests.Consumers;

public class OrderReadyForPickupConsumerTests : IClassFixture<CourierWorkerApplicationFactory>
{
    private readonly CourierWorkerApplicationFactory _factory;

    public OrderReadyForPickupConsumerTests(CourierWorkerApplicationFactory factory)
    {
        _factory = factory;
        _factory.PushNotificationServiceMock.Invocations.Clear();
    }

    [Fact]
    public async Task Consume_ShouldNotifyCouriers_WhenTheyAreWithinSearchRadiusandisOflline()
    {
        // Arrange - خواندن سرویس‌ها مستقیماً از نمونه هاست مستقل تست

        var redis = _factory.TestHost.Services.GetRequiredService<IConnectionMultiplexer>().GetDatabase();
        var harness = _factory.TestHost.Services.GetRequiredService<ITestHarness>();


        var restaurantLat = 52.5200;
        var restaurantLng = 13.4050;

        var closeCourierId = Guid.NewGuid().ToString();
        var farCourierId = Guid.NewGuid().ToString();

        // تزریق دیتا به ریدیس واقعی داکر کانتینر
        await redis.GeoAddAsync("couriers:locations", 13.4250, 52.5250, closeCourierId);
        await redis.GeoAddAsync("couriers:locations", 13.1000, 52.6000, farCourierId);
        await redis.StringSetAsync($"presence:courier:{closeCourierId}", "Offline");

        var testEvent = OrderReadyForPickupIntegrationEvent.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Burger King", DateTime.UtcNow,
            "Berlin Center", restaurantLat, restaurantLng,
            "Customer Address", 52.5300, 13.4100);

        // Act
        // ۱. پابلیش کردن پیام
        await harness.Bus.Publish(testEvent, CancellationToken.None);

        // Assert
        // 🚀 تغییر طلایی اول: به مس‌ترنزیت می‌گوییم تا زمانی که مطمئن نشده پیام توسط کانسیومر مصرف شده، خط بعدی را اجرا نکند
        // این متد به صورت هوشمند منتظر می‌ماند و به محض مصرف شدن پیام، تاییدیه می‌دهد (حداکثر تا ۵ ثانیه زمان می‌گذارد)
        var isConsumed = await harness.Consumed.SelectAsync<OrderReadyForPickupIntegrationEvent>(CancellationToken.None).Any();
        isConsumed.Should().BeTrue("Consumer must catch and handle the integration event from the broker.");

        // 🚀 تغییر طلایی دوم: یک تاخیر بسیار کوتاه (مثلاً ۲۰۰ میلی‌ثانیه) می‌دهیم تا منطق داخلی کدهای کانسیومر 
        // (مثل دسترسی به ریدیس و زدن متد SendPushAsync) کاملاً تکمیل شود و Mock پر شود.
        //  await Task.Delay(2000, CancellationToken.None);

        // Assert
        // حالا با خیال راحت تاییدیه متد نوتیفیکیشن را چک می‌کنیم
        _factory.PushNotificationServiceMock.Verify(
            x => x.SendPushAsync(
                closeCourierId,
                It.IsAny<string>(),
                It.IsAny<object>()),
            Times.Once,
            "Close courier should receive a push notification.");

        _factory.PushNotificationServiceMock.Verify(
            x => x.SendPushAsync(
                farCourierId,
                It.IsAny<string>(),
                It.IsAny<object>()),
            Times.Never,
            "Far courier should NOT receive a push notification.");
    }


    [Fact]
    public async Task Consume_ShouldNotifyOnlyClosestCouriers_WhenCouriersExistInMultipleRadiusRanges()
    {
        // Arrange - خواندن سرویس‌ها از هاست برنده و آماده تست
        var redis = _factory.TestHost.Services.GetRequiredService<IConnectionMultiplexer>().GetDatabase();
        var harness = _factory.TestHost.Services.GetRequiredService<ITestHarness>();

        // مختصات رستوران (مرکز فرضی)
        var restaurantLat = 52.5200;
        var restaurantLng = 13.4050;

        var veryCloseCourierId = $"Courier_Close_{Guid.NewGuid()}";
        var boundaryCourierId = $"Courier_Boundary_{Guid.NewGuid()}";
        var tooFarCourierId = $"Courier_Far_{Guid.NewGuid()}";

        // ۱. پیک بسیار نزدیک - فاصله حدود ۱.۵ کیلومتر (داخل شعاع ۳ کیلومتر اولیه)
        await redis.GeoAddAsync("couriers:locations", 13.4250, 52.5250, veryCloseCourierId);

        // ۲. پیک مرزی - فاصله حدود ۵.۵ کیلومتر (خارج از شعاع ۳، داخل شعاع ۶ کیلومتر دوم)
        await redis.GeoAddAsync("couriers:locations", 13.4850, 52.5450, boundaryCourierId);

        // ۳. پیک بسیار دور - فاصله حدود ۱۵ کیلومتر (خارج از هر دو شعاع جستجو)
        await redis.GeoAddAsync("couriers:locations", 13.1000, 52.6000, tooFarCourierId);

        // تنظیم وضعیت پیک‌ها به آفلاین برای وریفای راحت‌تر روی PushNotificationServiceMock
        await redis.StringSetAsync($"presence:courier:{veryCloseCourierId}", "Offline");
        await redis.StringSetAsync($"presence:courier:{boundaryCourierId}", "Offline");
        await redis.StringSetAsync($"presence:courier:{tooFarCourierId}", "Offline");

        // ساخت پیام اینتگریشن ایونت غذا آماده است
        var testEvent = OrderReadyForPickupIntegrationEvent.Create(
            Guid.NewGuid(), Guid.NewGuid(), "McDonald's", DateTime.UtcNow,
            "Berlin Alexanderplatz", restaurantLat, restaurantLng,
            "Customer House", 52.5300, 13.4100);

        // Act
        // شلیک پیام به مموری بوروکر مس‌ترنزیت
        await harness.Bus.Publish(testEvent, CancellationToken.None);

        // Assert
        // تایید اینکه مس‌ترنزیت پیام را به کانسیومر رسانده است
        var isConsumed = await harness.Consumed.SelectAsync<OrderReadyForPickupIntegrationEvent>(CancellationToken.None).Any();
        isConsumed.Should().BeTrue("The consumer must catch and execute the notification logic.");

        // چون کدهای تو منطق تاخیر ۲۰۰ میلی‌ثانیه‌ای را نیاز دارند، اینجا هم حفظش می‌کنیم تا گند به ترکیب برنده نخورد
        await Task.Delay(200);

        // ۱. تایید قطعی اینکه نزدیک‌ترین پیک حتماً نوتیفیکیشن را دقیقاً ۱ بار دریافت کرده است
        _factory.PushNotificationServiceMock.Verify(
            x => x.SendPushAsync(
                veryCloseCourierId,
                It.IsAny<string>(),
                It.IsAny<object>()),
            Times.Once,
            "The closest courier within the 3km radius must be notified.");

        // ۲. تایید اینکه پیک محدوده دوم (۵.۵ کیلومتری) اصلاً نباید پیام بگیرد، چون کدت شرط گذاشته اگر در شعاع اول کسی بود، همان‌جا تمام شود
        _factory.PushNotificationServiceMock.Verify(
            x => x.SendPushAsync(
                boundaryCourierId,
                It.IsAny<string>(),
                It.IsAny<object>()),
            Times.Never,
            "The courier at 5.5km should NOT be notified because a closer courier was already found in the 3km target.");

        // ۳. تایید اینکه پیک دور افتاده (۱۵ کیلومتری) کلاً هیچ چیزی دریافت نکرده است
        _factory.PushNotificationServiceMock.Verify(
            x => x.SendPushAsync(
                tooFarCourierId,
                It.IsAny<string>(),
                It.IsAny<object>()),
            Times.Never,
            "The far away courier must never receive any notifications.");
    }

    [Fact]
    public async Task Consume_ShouldSendSignalRMessageToClosestCourier_WhenCourierIsOnline()
    {
        // Arrange
        var redis = _factory.TestHost.Services.GetRequiredService<IConnectionMultiplexer>().GetDatabase();
        var harness = _factory.TestHost.Services.GetRequiredService<ITestHarness>();

        // ریست کردن موک سیگنال‌آر قبل از شروع این تست
        _factory.HubClientsMock.Invocations.Clear();
        _factory.ClientProxyMock.Invocations.Clear();

        var restaurantLat = 52.5200;
        var restaurantLng = 13.4050;

        var onlineCloseCourierId = $"Courier_Online_{Guid.NewGuid()}";
        var offlineFarCourierId = $"Courier_Offline_{Guid.NewGuid()}";

        // ۱. پیک نزدیک در فاصله ۱.۵ کیلومتری رستوران
        await redis.GeoAddAsync("couriers:locations", 13.4250, 52.5250, onlineCloseCourierId);

        // ۲. پیک دور در فاصله ۱۵ کیلومتری رستوران
        await redis.GeoAddAsync("couriers:locations", 13.1000, 52.6000, offlineFarCourierId);

        // 🚀 نکته اصلی سناریو: وضعیت پیک نزدیک "Online" است (اپ باز است)
        await redis.StringSetAsync($"presence:courier:{onlineCloseCourierId}", "Online");
        await redis.StringSetAsync($"presence:courier:{offlineFarCourierId}", "Offline");

        var testEvent = OrderReadyForPickupIntegrationEvent.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Pizza Hut", DateTime.UtcNow,
            "Berlin Potsdamer Platz", restaurantLat, restaurantLng,
            "Customer Location", 52.5300, 13.4100);

        // Act
        await harness.Bus.Publish(testEvent, CancellationToken.None);

        // منتظر می‌مانیم تا مس‌ترنزیت پیام را کامل به کانسیومر تحویل دهد
        var isConsumed = await harness.Consumed.SelectAsync<OrderReadyForPickupIntegrationEvent>(CancellationToken.None).Any();
        isConsumed.Should().BeTrue("The consumer must execute for the online courier.");

        // تاخیر ۲۰۰ میلی‌ثانیه‌ای تضمین‌شده برای حفظ ترکیب برنده کدهای شما
        //    await Task.Delay(200);

        // Assert
        // ۱. بررسی اینکه سیگنال‌آر تلاش کرده پیام را دقیقاً به گروه اختصاصی این پیک بفرستد: "Courier_{courierId}"
        _factory.HubClientsMock.Verify(
            x => x.Group($"Courier_{onlineCloseCourierId}"),
            Times.Once,
            "SignalR must target the specific group of the closest online courier.");

        // ۲. بررسی اینکه متد SendCoreAsync (که پشت صحنه SendAsync در سیگنال‌آر است) با نام متد "ReceiveAvailableOrder" صدا زده شده است
        _factory.ClientProxyMock.Verify(
            x => x.SendCoreAsync(
                "ReceiveAvailableOrder",
                It.Is<object[]>(args => args.Length == 1), // تایید اینکه آبجکت Payload پاس داده شده است
                It.IsAny<CancellationToken>()),
            Times.Once,
            "The real-time order payload must be dispatched via the SignalR group proxy.");

        // ۳. مطمئن می‌شویم که برای این پیکِ آنلاین، هیچ پُش‌نوتیفیکیشنی (Firebase) ارسال نشده است!
        _factory.PushNotificationServiceMock.Verify(
            x => x.SendPushAsync(
                onlineCloseCourierId,
                It.IsAny<string>(),
                It.IsAny<object>()),
            Times.Never,
            "An online courier should only get websocket updates, not background push notifications.");
    }
}