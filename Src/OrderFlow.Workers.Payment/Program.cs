using MassTransit;
using OrderFlow.Workers.Payment.Consumers;
using Serilog;

namespace OrderFlow.Workers.Payment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            var rabbitMq = builder.Configuration.GetSection("RabbitMQ");
            var seq = builder.Configuration.GetSection("Seq");

            builder.Services.AddSerilog((services, configuration) => configuration
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "PaymentWorker")
                .WriteTo.Console()
                .WriteTo.Seq(seq["Url"]!));

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderPlacedConsumer>();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMq["Host"], rabbitMq["VirtualHost"], h =>
                    {
                        h.Username(rabbitMq["Username"]!);
                        h.Password(rabbitMq["Password"]!);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });

            var host = builder.Build();
            host.Run();
        }
    }
}