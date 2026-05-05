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
                    Port = 5672,
                    UserName = "guest",
                    Password = "guest"

                };
                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            });

            builder.Services.AddScoped<PaymentProcessor>();


            var host = builder.Build();
            host.Run();
        }
    }
}