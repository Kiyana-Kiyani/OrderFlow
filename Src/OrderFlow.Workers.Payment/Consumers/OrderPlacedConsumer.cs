using OrderFlow.Contracts.IntegrationEvents;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace OrderFlow.Workers.Payment.Consumers
{
    public class OrderPlacedConsumer : BackgroundService
    {
        private readonly ILogger<OrderPlacedConsumer> _logger;
        private readonly IConnection _connection;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private IChannel? _channel;

        private const string QueueName = "payment.processing.queue";
        private const string ExchangeName = "orderflow.events";
        private const string RoatingKey = "orderplacedintegrationevent";

        public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger, IConnection connection, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _connection = connection;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Fanout, true, false, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync(QueueName, true, false, false, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(QueueName, ExchangeName, RoatingKey, null, cancellationToken: stoppingToken);
            await _channel.BasicQosAsync(0, 1, false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var process = scope.ServiceProvider.GetRequiredService<PaymentProcessor>();

                try
                {
                    var body = eventArgs.Body.ToArray();
                    var orderPlacedEvent = JsonSerializer.Deserialize<OrderPlacedIntegrationEvent>(body);

                    if (orderPlacedEvent is not null)
                    {

                        process.HandleAsync(orderPlacedEvent, stoppingToken).GetAwaiter().GetResult();
                    }

                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken: stoppingToken);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {Message}", ex.Message);
                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken: stoppingToken);

                }
            };
            await _channel.BasicConsumeAsync(QueueName, false, consumer, cancellationToken: stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null) await _channel.CloseAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}

