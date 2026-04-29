using OrderFlow.Application.Abstractions.Messaging;
using OrderFlow.Contracts.IntegrationEvents;
using RabbitMQ.Client;
using System.Text.Json;

namespace OrderFlow.Infrastructure.RabbitMQ
{
    public class RabbitMqEventPublisher : IEventPublisher
    {
        private readonly IConnection _connection;
        private const string ExchangeName = "orderflow.events";

        public RabbitMqEventPublisher(IConnection connection)
        {
            _connection = connection;
        }

        public async Task PublishAsync<T>(T intrgrationEvent, CancellationToken cancellationToken = default) where T : IIntegrationEvent
        {
            using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);

            var properties = new BasicProperties
            {
                MessageId = intrgrationEvent.EventId.ToString(),
                Persistent = true,
                Timestamp = new AmqpTimestamp(((DateTimeOffset)intrgrationEvent.OccurredOnUtc).ToUnixTimeSeconds())
            };

            var body = JsonSerializer.SerializeToUtf8Bytes(intrgrationEvent, intrgrationEvent.GetType());
            var routingKey = intrgrationEvent.GetType().Name.ToLower();

            await channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                mandatory: false,
                body: body,
                cancellationToken: cancellationToken
                );
        }
    }
}
