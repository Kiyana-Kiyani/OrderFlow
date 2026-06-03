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

        // Arrange - بازیابی ساختار تست‌هارنس از هاست فعال ورکر پرداخت
        var harness = _factory.TestHost.Services.GetRequiredService<ITestHarness>();

        var orderId = Guid.NewGuid();
        var customerUserId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var totalAmount = 75.50m;

        // شبیه‌سازی دقیق ساخت رویداد اولیه ثبت سفارش مطابق با کلاس Contract شما
        var orderPlacedEvent = OrderPlacedIntegrationEvent.CreateNew(
            orderId,
            customerUserId,
            restaurantId,
            totalAmount
        );

        // Act - شلیک رویداد به شبکه داخلی مس‌ترنزیت
        await harness.Bus.Publish(orderPlacedEvent, CancellationToken.None);

        // Assert - ۱. تایید اینکه پیام توسط کانسیومر ورکر برداشته و مصرف شده است
        var isConsumed = await harness.Consumed
            .SelectAsync<OrderPlacedIntegrationEvent>(CancellationToken.None)
            .Any();

        isConsumed.Should().BeTrue("The Payment worker's OrderPlacedConsumer must consume the event.");

        // ۲. مدیریت منطق رندوم: در هر دو حالت موفقیت یا شکست، سیستم باید اِونتِ متناظر خروجی را صادر کرده باشد
        var isPaymentSucceededEventPublished = await harness.Published
            .Any<PaymentSucceededIntegrationEvent>(x => x.Context.Message.OrderId == orderId, ct);

        var isPaymentFailedEventPublished = await harness.Published
            .Any<PaymentFailedIntegrationEvent>(x => x.Context.Message.OrderId == orderId, ct);

        // تایید نهایی پایپ‌لاین: حتماً باید یکی از دو وضعیت بیزینسی (پرداخت موفق / ناموفق) در شبکه فریاد زده شده باشد
        (isPaymentSucceededEventPublished || isPaymentFailedEventPublished)
            .Should()
            .BeTrue("The payment consumer must result in publishing either PaymentSucceeded or PaymentFailed integration events.");
    }
}