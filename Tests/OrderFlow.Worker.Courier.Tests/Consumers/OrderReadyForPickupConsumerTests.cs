using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrderFlow.Contracts.IntegrationEvents;
using OrderFlow.Worker.Courier.Tests.Fixtures;
using StackExchange.Redis;

namespace OrderFlow.Worker.Courier.Tests.Consumers;

public class OrderReadyForPickupConsumerTests : IClassFixture<CourierWorkerApplicationFactory>
{
    private readonly CourierWorkerApplicationFactory _factory;

    public OrderReadyForPickupConsumerTests(CourierWorkerApplicationFactory factory)
    {
        _factory = factory;

        _factory.PushNotificationServiceMock.Invocations.Clear();
        _factory.HubClientsMock.Invocations.Clear();
        _factory.ClientProxyMock.Invocations.Clear();

        var redis = _factory.TestHost.Services.GetRequiredService<IConnectionMultiplexer>().GetDatabase();
        redis.Execute("FLUSHDB");
    }

    [Fact]
    public async Task Consume_ShouldNotifyCouriers_WhenTheyAreWithinSearchRadiusandisOflline()
    {
        var ct = TestContext.Current.CancellationToken;
        // Arrange
        var redis = _factory.TestHost.Services.GetRequiredService<IConnectionMultiplexer>().GetDatabase();
        var harness = _factory.TestHost.Services.GetRequiredService<ITestHarness>();

        var restaurantLat = 52.5200;
        var restaurantLng = 13.4050;
        var orderId = Guid.NewGuid();

        var closeCourierId = Guid.NewGuid().ToString();
        var farCourierId = Guid.NewGuid().ToString();

        await redis.GeoAddAsync("couriers:locations", 13.4250, 52.5250, closeCourierId);
        await redis.GeoAddAsync("couriers:locations", 13.1000, 52.6000, farCourierId);
        await redis.StringSetAsync($"presence:courier:{closeCourierId}", "Offline");

        var testEvent = OrderReadyForPickupIntegrationEvent.Create(
            orderId, Guid.NewGuid(), "Burger King", DateTime.UtcNow,
            "Berlin Center", restaurantLat, restaurantLng,
            "Customer Address", 52.5300, 13.4100);

        // Act
        await harness.Bus.Publish(testEvent, ct);

        // Assert
        var isConsumed = await harness.Consumed.Any<OrderReadyForPickupIntegrationEvent>(
            x => x.Context.Message.OrderId == orderId, ct);
        isConsumed.Should().BeTrue("Consumer must handle the specific event for this test.");

        await Task.Delay(200, ct);

        _factory.PushNotificationServiceMock.Verify(
            x => x.SendPushAsync(closeCourierId, It.IsAny<string>(), It.IsAny<object>()),
            Times.Once,
            "Close courier should receive a push notification.");

        _factory.PushNotificationServiceMock.Verify(
            x => x.SendPushAsync(farCourierId, It.IsAny<string>(), It.IsAny<object>()),
            Times.Never,
            "Far courier should NOT receive a push notification.");
    }

    [Fact]
    public async Task Consume_ShouldNotifyOnlyClosestCouriers_WhenCouriersExistInMultipleRadiusRanges()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange
        var redis = _factory.TestHost.Services.GetRequiredService<IConnectionMultiplexer>().GetDatabase();
        var harness = _factory.TestHost.Services.GetRequiredService<ITestHarness>();

        var restaurantLat = 52.5200;
        var restaurantLng = 13.4050;
        var orderId = Guid.NewGuid();

        var veryCloseCourierId = $"Courier_Close_{Guid.NewGuid()}";
        var boundaryCourierId = $"Courier_Boundary_{Guid.NewGuid()}";
        var tooFarCourierId = $"Courier_Far_{Guid.NewGuid()}";

        await redis.GeoAddAsync("couriers:locations", 13.4250, 52.5250, veryCloseCourierId);
        await redis.GeoAddAsync("couriers:locations", 13.4850, 52.5450, boundaryCourierId);
        await redis.GeoAddAsync("couriers:locations", 13.1000, 52.6000, tooFarCourierId);

        await redis.StringSetAsync($"presence:courier:{veryCloseCourierId}", "Offline");
        await redis.StringSetAsync($"presence:courier:{boundaryCourierId}", "Offline");
        await redis.StringSetAsync($"presence:courier:{tooFarCourierId}", "Offline");

        var testEvent = OrderReadyForPickupIntegrationEvent.Create(
            orderId, Guid.NewGuid(), "McDonald's", DateTime.UtcNow,
            "Berlin Alexanderplatz", restaurantLat, restaurantLng,
            "Customer House", 52.5300, 13.4100);

        // Act
        await harness.Bus.Publish(testEvent, ct);

        // Assert
        var isConsumed = await harness.Consumed.Any<OrderReadyForPickupIntegrationEvent>(
            x => x.Context.Message.OrderId == orderId, ct);
        isConsumed.Should().BeTrue("The consumer must catch and execute the notification logic.");

        await Task.Delay(200, ct);

        _factory.PushNotificationServiceMock.Verify(
            x => x.SendPushAsync(veryCloseCourierId, It.IsAny<string>(), It.IsAny<object>()),
            Times.Once,
            "The closest courier within the 3km radius must be notified.");

        _factory.PushNotificationServiceMock.Verify(
            x => x.SendPushAsync(boundaryCourierId, It.IsAny<string>(), It.IsAny<object>()),
            Times.Never,
            "The courier at 5.5km should NOT be notified because a closer courier was already found.");

        _factory.PushNotificationServiceMock.Verify(
            x => x.SendPushAsync(tooFarCourierId, It.IsAny<string>(), It.IsAny<object>()),
            Times.Never,
            "The far away courier must never receive any notifications.");
    }

    [Fact]
    public async Task Consume_ShouldSendSignalRMessageToClosestCourier_WhenCourierIsOnline()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange
        var redis = _factory.TestHost.Services.GetRequiredService<IConnectionMultiplexer>().GetDatabase();
        var harness = _factory.TestHost.Services.GetRequiredService<ITestHarness>();

        var restaurantLat = 52.5200;
        var restaurantLng = 13.4050;
        var orderId = Guid.NewGuid();

        var onlineCloseCourierId = $"Courier_Online_{Guid.NewGuid()}";
        var offlineFarCourierId = $"Courier_Offline_{Guid.NewGuid()}";

        await redis.GeoAddAsync("couriers:locations", 13.4250, 52.5250, onlineCloseCourierId);
        await redis.GeoAddAsync("couriers:locations", 13.1000, 52.6000, offlineFarCourierId);

        await redis.StringSetAsync($"presence:courier:{onlineCloseCourierId}", "Online");
        await redis.StringSetAsync($"presence:courier:{offlineFarCourierId}", "Offline");

        var testEvent = OrderReadyForPickupIntegrationEvent.Create(
            orderId, Guid.NewGuid(), "Pizza Hut", DateTime.UtcNow,
            "Berlin Potsdamer Platz", restaurantLat, restaurantLng,
            "Customer Location", 52.5300, 13.4100);

        // Act
        await harness.Bus.Publish(testEvent, ct);

        // Assert
        var isConsumed = await harness.Consumed.Any<OrderReadyForPickupIntegrationEvent>(
            x => x.Context.Message.OrderId == orderId, ct);
        isConsumed.Should().BeTrue("The consumer must execute for the online courier.");

        await Task.Delay(200, ct);

        _factory.HubClientsMock.Verify(
            x => x.Group($"Courier_{onlineCloseCourierId}"),
            Times.Once,
            "SignalR must target the specific group of the closest online courier.");

        _factory.ClientProxyMock.Verify(
            x => x.SendCoreAsync(
                "ReceiveAvailableOrder",
                It.Is<object[]>(args => args.Length == 1),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "The real-time order payload must be dispatched via the SignalR group proxy.");

        _factory.PushNotificationServiceMock.Verify(
            x => x.SendPushAsync(onlineCloseCourierId, It.IsAny<string>(), It.IsAny<object>()),
            Times.Never,
            "An online courier should only get websocket updates.");
    }
}