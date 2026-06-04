using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Contracts.IntegrationEvents;
using OrderFlow.Workers.Payment.Tests.Fixtures;

namespace OrderFlow.Workers.Payment.Tests.Consumers;

public class OrderPlacedConsumerTests : IClassFixture<PaymentWorkerApplicationFactory>
{
    private readonly PaymentWorkerApplicationFactory _factory;

    public OrderPlacedConsumerTests(PaymentWorkerApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Consume_ShouldProcessOrderPlacedEvent_AndPublishPaymentResultEvent()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange
        var harness = _factory.TestHost.Services.GetRequiredService<ITestHarness>();

        var orderId = Guid.NewGuid();
        var customerUserId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var totalAmount = 75.50m;

        var orderPlacedEvent = OrderPlacedIntegrationEvent.CreateNew(
            orderId,
            customerUserId,
            restaurantId,
            totalAmount
        );

        // Act
        await harness.Bus.Publish(orderPlacedEvent, CancellationToken.None);

        // Assert 
        var isConsumed = await harness.Consumed
            .SelectAsync<OrderPlacedIntegrationEvent>(CancellationToken.None)
            .Any();

        isConsumed.Should().BeTrue("The Payment worker's OrderPlacedConsumer must consume the event.");

        var isPaymentSucceededEventPublished = await harness.Published
            .Any<PaymentSucceededIntegrationEvent>(x => x.Context.Message.OrderId == orderId, ct);

        var isPaymentFailedEventPublished = await harness.Published
            .Any<PaymentFailedIntegrationEvent>(x => x.Context.Message.OrderId == orderId, ct);

        (isPaymentSucceededEventPublished || isPaymentFailedEventPublished)
            .Should()
            .BeTrue("The payment consumer must result in publishing either PaymentSucceeded or PaymentFailed integration events.");
    }
}