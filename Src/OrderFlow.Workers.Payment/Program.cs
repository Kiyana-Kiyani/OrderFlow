using OrderFlow.Workers.Payment.Consumers;
using RabbitMQ.Client;

namespace OrderFlow.Workers.Payment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<OrderPlacedConsumer>();

            builder.Services.AddSingleton<IConnection>(opt =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = "localhost",
                };
                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            });

            //ddd


            builder.Services.AddScoped<PaymentProcessor>();


            var host = builder.Build();
            host.Run();
        }
    }
}