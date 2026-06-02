using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderFlow.Worker.Courier.Abstractions;
using OrderFlow.Worker.Courier.Consumers;
using OrderFlow.Worker.Courier.Infrastructure;
using Serilog;
using StackExchange.Redis;

namespace OrderFlow.Worker.Courier
{
    public class Program
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
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = ConfigurationOptions.Parse(redis["ConnectionString"]!);
                configuration.AbortOnConnectFail = false; // اگر بار اول وصل نشد، کرش نکن و در پس‌زمینه تلاش کن
                return ConnectionMultiplexer.Connect(configuration);
            });


            builder.Services.AddTransient<IPushNotificationService, FirebasePushNotificationService>();


            // ⚡ فیکس نهایی: متصل کردن سیگنال‌آرِ ورکر به بک‌پلیین مشترک ردیس (بدون پروکسی مس‌ترنزیت)
            builder.Services.AddSignalR()
                .AddStackExchangeRedis(redis["ConnectionString"]!, options =>
                {
                    // این همان کانالی است که API هم به آن گوش می‌دهد
                    options.Configuration.ChannelPrefix = RedisChannel.Literal("OrderFlow_WebSockets");
                });

            // 6. Configure MassTransit utilizing standard Default Topologies
            builder.Services.AddMassTransit(x =>
            {
                // Register your courier background consumer class type
                x.AddConsumer<OrderReadyForPickupConsumer>();

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
