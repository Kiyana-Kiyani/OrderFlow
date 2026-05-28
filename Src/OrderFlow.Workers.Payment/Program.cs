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

                    // ۷. استفاده از Outbox در حافظه برای اطمینان از تحویل ایمن پیام‌ها حتی در صورت بروز خطاهای موقتی
                    cfg.UseInMemoryOutbox(context);

                    cfg.ConfigureEndpoints(context);


                });
            });



            var host = builder.Build();
            host.Run();
        }
    }
}
//builder.Services.AddMassTransit(x =>
//{
//    x.AddConsumer<OrderPlacedConsumer>();
//    x.UsingRabbitMq((context, cfg) =>
//    {
//        cfg.Host(rabbitMq["Host"], rabbitMq["VirtualHost"], h =>
//        {
//            h.Username(rabbitMq["Username"]!);
//            h.Password(rabbitMq["Password"]!);
//        });
//        cfg.UseMessageRetry(r =>
//        {
//            r.Interval(3, TimeSpan.FromSeconds(2));
//        });

//        cfg.ReceiveEndpoint("orderflow-payment-queue", e =>
//        {
//            e.SetQuorumQueue();
//            e.ConfigureConsumeTopology = false;
//            e.Bind("orderflow.events", s =>
//            {
//                s.RoutingKey = "orderplaced";
//                s.ExchangeType = "topic";
//            });
//            e.ConfigureConsumer<OrderPlacedConsumer>(context);
//        });

//        cfg.Message<PaymentSucceededIntegrationEvent>(x => x.SetEntityName("Payment.Result"));
//        cfg.Publish<PaymentSucceededIntegrationEvent>(x =>
//        {
//            x.ExchangeType = "topic";
//            x.Durable = true;
//        });

//        cfg.Message<PaymentFailedIntegrationEvent>(x => x.SetEntityName("Payment.Result"));
//        cfg.Publish<PaymentFailedIntegrationEvent>(x =>
//        {
//            x.ExchangeType = "topic";
//            x.Durable = true;
//        });


//    });
//});
