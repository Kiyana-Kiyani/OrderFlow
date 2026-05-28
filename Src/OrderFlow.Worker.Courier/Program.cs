using MassTransit;
using MassTransit.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderFlow.Contracts.Hubs;
using OrderFlow.Worker.Courier.Abstractions;
using OrderFlow.Worker.Courier.Consumers;
using OrderFlow.Worker.Courier.Infrastructure;
using Serilog;
using StackExchange.Redis;

namespace OrderFlow.Worker.Courier
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Aligned with your modern .NET ApplicationBuilder setup
            var builder = Host.CreateApplicationBuilder(args);

            // Extracting configuration sections exactly like your Payment worker
            var rabbitMq = builder.Configuration.GetSection("RabbitMQ");
            var redis = builder.Configuration.GetSection("Redis");
            var seq = builder.Configuration.GetSection("Seq");

            // Aligned Logging Infrastructure
            builder.Services.AddSerilog((services, configuration) => configuration
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "CourierWorker")
                .WriteTo.Console()
                .WriteTo.Seq(seq["Url"]!));


            // ۱. ثبت اتصال نیتیو ردیس برای محاسبات جئو (Geo)
            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redis["ConnectionString"]!));


            builder.Services.AddTransient<IPushNotificationService, FirebasePushNotificationService>();


            // ⚡ فیکس اصلی: ثبت هسته مرکزی سیگنال‌آر در پروژه ورکر ⚡
            builder.Services.AddSignalR();

            // 6. Configure MassTransit utilizing standard Default Topologies
            builder.Services.AddMassTransit(x =>
            {
                // Register your courier background consumer class type
                x.AddConsumer<OrderReadyForPickupConsumer>();

                // ⚡ ثبت پروکسی سیگنال‌آر: به ورکر اجازه می‌دهد پیام را به هابِ پروژه API هدایت کند
                // ثبت پروکسی سیگنال‌آر مس‌ترنزیت روی هسته اصلی
                x.AddSignalRHub<OrderHub>();

                // Automatically format your queues to follow clean web standards (e.g., "orderflow-food-ready")
                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) =>
                {
                    // Establish connection credentials
                    cfg.Host(rabbitMq["Host"], rabbitMq["VirtualHost"], h =>
                    {
                        h.Username(rabbitMq["Username"]!);
                        h.Password(rabbitMq["Password"]!);
                    });

                    // Match your exact retry resilience policy specs
                    cfg.UseMessageRetry(r =>
                    {
                        r.Interval(3, TimeSpan.FromSeconds(2));
                    });
                    // ۷. استفاده از Outbox در حافظه برای اطمینان از تحویل ایمن پیام‌ها حتی در صورت بروز خطاهای موقتی
                    cfg.UseInMemoryOutbox(context);
                    // THE MAGIC DEFAULT COMMAND:
                    // Automatically builds the 'orderflow-food-ready' queue as a durable Quorum line,
                    // discovers that it consumes 'FoodReadyIntegrationEvent', discovers the default 
                    // fanout exchange, and binds them together perfectly behind the scenes.
                    cfg.ConfigureEndpoints(context);
                });
            });
            var host = builder.Build();
            await host.RunAsync();
        }
    }
}
