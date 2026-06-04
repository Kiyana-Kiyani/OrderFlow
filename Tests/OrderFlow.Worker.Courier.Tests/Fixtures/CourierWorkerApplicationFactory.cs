using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using OrderFlow.Contracts.Hubs;
using OrderFlow.Worker.Courier.Abstractions;
using OrderFlow.Worker.Courier.Consumers;
using Serilog;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace OrderFlow.Worker.Courier.Tests.Fixtures;

public class CourierWorkerApplicationFactory : IAsyncLifetime
{
    private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7-alpine")
        .WithName("courier-worker-redis-test")
        .Build();

    public Mock<IPushNotificationService> PushNotificationServiceMock { get; } = new();
    public Mock<IHubClients> HubClientsMock { get; } = new();
    public Mock<IClientProxy> ClientProxyMock { get; } = new();

    public IHost TestHost { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _redisContainer.StartAsync();
        HubClientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(ClientProxyMock.Object);
        var hubContextMock = new Mock<IHubContext<OrderHub>>();
        hubContextMock.Setup(x => x.Clients).Returns(HubClientsMock.Object);

        TestHost = Host.CreateDefaultBuilder()
            .UseEnvironment("Testing")
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Redis:ConnectionString", _redisContainer.GetConnectionString() },
                    { "Seq:Url", "http://localhost:5341" }
                });
            })
            .UseSerilog((context, services, configuration) => configuration
                .WriteTo.Console()
                .MinimumLevel.Debug())
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(PushNotificationServiceMock.Object);

                services.AddSingleton<IConnectionMultiplexer>(
                    ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString()));

                services.AddSignalR();

                services.RemoveAll<IHubContext<OrderHub>>();
                services.AddSingleton(hubContextMock.Object);

                services.AddMassTransitTestHarness(x =>
                {
                    x.AddConsumer<OrderReadyForPickupConsumer>();
                });
            })
            .Build();

        await TestHost.StartAsync();
    }
    public async ValueTask DisposeAsync()
    {
        if (TestHost != null)
        {
            await TestHost.StopAsync();
            TestHost.Dispose();
        }

        await _redisContainer.DisposeAsync();
        await Log.CloseAndFlushAsync();
    }
}






//using MassTransit;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Moq;
//using OrderFlow.Worker.Courier.Abstractions;
//using OrderFlow.Worker.Courier.Consumers;
//using Serilog;
//using StackExchange.Redis;
//using Testcontainers.Redis;

//namespace OrderFlow.Worker.Courier.Tests.Fixtures;

//public class CourierWorkerApplicationFactory : IAsyncLifetime
//{
//    private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7-alpine")
//        .WithName("courier-worker-redis-test")
//        .Build();

//    public Mock<IPushNotificationService> PushNotificationServiceMock { get; } = new();

//    // هورست اختصاصی ما برای محیط تست
//    public IHost TestHost { get; private set; } = null!;
//    public Mock<IHubClients> HubClientsMock { get; } = new();
//    public Mock<IClientProxy> ClientProxyMock { get; } = new();
//    public async ValueTask InitializeAsync()
//    {
//        // ۱. ابتدا استارت کانتینر واقعی ریدیس داکر
//        await _redisContainer.StartAsync();

//        // ۲. ساخت یک Generic Host واقعی کپی پروداکشن اما کاملاً سفارشی‌شده برای تست
//        TestHost = Host.CreateDefaultBuilder()
//            .UseEnvironment("Testing")
//            .ConfigureAppConfiguration((context, config) =>
//            {
//                config.AddInMemoryCollection(new Dictionary<string, string?>
//                {
//                    { "Redis:ConnectionString", _redisContainer.GetConnectionString() },
//                    { "Seq:Url", "http://localhost:5341" }
//                });
//            })
//            .UseSerilog((context, services, configuration) => configuration
//                .WriteTo.Console()
//                .MinimumLevel.Debug())
//            .ConfigureServices((context, services) =>
//            {
//                // ثبت موک نوتیفیکیشن
//                services.AddSingleton(PushNotificationServiceMock.Object);

//                // اتصال ریدیس به کانتینر واقعی داکر
//                services.AddSingleton<IConnectionMultiplexer>(
//                    ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString()));

//                // 🚀 اضافه شد: ثبت سیگنال‌آر در محیط تست برای زنده کردن کانسیومر
//                services.AddSignalR();

//                // پیکربندی MassTransit با استفاده از هارنس تست داخلی (بدون نیاز به وب فکتوری)
//                services.AddMassTransitTestHarness(x =>
//                {
//                    x.AddConsumer<OrderReadyForPickupConsumer>();
//                });

//            })
//            .Build();

//        // ۳. استارت زدن هاست تست در پس‌زمینه
//        await TestHost.StartAsync();
//    }

//    public async ValueTask DisposeAsync()
//    {
//        if (TestHost != null)
//        {
//            await TestHost.StopAsync();
//            TestHost.Dispose();
//        }

//        await _redisContainer.DisposeAsync();
//        await Log.CloseAndFlushAsync();
//    }
//}