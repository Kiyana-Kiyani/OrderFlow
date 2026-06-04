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
            var builder = Host.CreateApplicationBuilder(args);

            var rabbitMq = builder.Configuration.GetSection("RabbitMQ");
            var redis = builder.Configuration.GetSection("Redis");
            var seq = builder.Configuration.GetSection("Seq");

            builder.Services.AddSerilog((services, configuration) => configuration
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "CourierWorker")
                .WriteTo.Console()
                .WriteTo.Seq(seq["Url"]!));


            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = ConfigurationOptions.Parse(redis["ConnectionString"]!);
                configuration.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(configuration);
            });


            builder.Services.AddTransient<IPushNotificationService, FirebasePushNotificationService>();


            builder.Services.AddSignalR()
                .AddStackExchangeRedis(redis["ConnectionString"]!, options =>
                {
                    options.Configuration.ChannelPrefix = RedisChannel.Literal("OrderFlow_WebSockets");
                });

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderReadyForPickupConsumer>();

                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMq["Host"], rabbitMq["VirtualHost"], h =>
                    {
                        h.Username(rabbitMq["Username"]!);
                        h.Password(rabbitMq["Password"]!);
                    });

                    cfg.UseMessageRetry(r =>
                    {
                        r.Interval(3, TimeSpan.FromSeconds(2));
                    });

                    cfg.UseInMemoryOutbox(context);
                    cfg.ConfigureEndpoints(context);
                });
            });
            var host = builder.Build();
            await host.RunAsync();
        }
    }
}
