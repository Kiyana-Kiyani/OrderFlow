using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OrderFlow.Workers.Payment.Consumers;
using Serilog;

namespace OrderFlow.Workers.Payment.Tests.Fixtures;

public class PaymentWorkerApplicationFactory : IAsyncLifetime
{
    public IHost TestHost { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        TestHost = Host.CreateDefaultBuilder()
            .UseEnvironment("Testing")
            .ConfigureAppConfiguration((context, config) =>
            {
                // شبیه‌سازی تنظیمات کانفیگ برای محیط تست
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RabbitMQ:Host", "localhost" },
                    { "RabbitMQ:Username", "guest" },
                    { "RabbitMQ:Password", "guest" },
                    { "RabbitMQ:VirtualHost", "/" },
                    { "Seq:Url", "http://localhost:5341" }
                });
            })
            .UseSerilog((context, services, configuration) => configuration
                .WriteTo.Console()
                .MinimumLevel.Debug())
            .ConfigureServices((context, services) =>
            {
                // راه‌اندازی تست‌هارنس در حافظه برای ماس‌ترنزیت
                services.AddMassTransitTestHarness(x =>
                {
                    x.AddConsumer<OrderPlacedConsumer>();
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

        await Log.CloseAndFlushAsync();
    }
}