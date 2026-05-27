using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderFlow.Contracts;
using OrderFlow.Worker.Courier.Consumers;
using OrderFlow.Worker.Courier.Infrastructure;
using Serilog;

namespace OrderFlow.Worker.Courier
{
    internal class Program
    {
        static void Main(string[] args)
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

            // Shared Redis Whiteboard Infrastructure
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redis["ConnectionString"] ?? "localhost:6379";
                options.InstanceName = "OrderFlow:";
            });

            builder.Services.AddScoped<ICourierConnectionTracker, CourierConnectionTracker>();

            // Standalone SignalR scale-out layer
            builder.Services.AddSignalR()
                    .AddStackExchangeRedis(redis["ConnectionString"] ?? "localhost:6379");


            // 6. Configure MassTransit utilizing standard Default Topologies
            builder.Services.AddMassTransit(x =>
            {
                // Register your courier background consumer class type
                x.AddConsumer<FoodReadyConsumer>();

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

                    // THE MAGIC DEFAULT COMMAND:
                    // Automatically builds the 'orderflow-food-ready' queue as a durable Quorum line,
                    // discovers that it consumes 'FoodReadyIntegrationEvent', discovers the default 
                    // fanout exchange, and binds them together perfectly behind the scenes.
                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}
