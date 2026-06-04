using FluentAssertions;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Contracts.IntegrationEvents;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.IntegrationTests.Fixtures;
using System.Net;

namespace OrderFlow.IntegrationTests.Features.Couriers;

public class CourierFlowIntegrationTests : BaseIntegrationTest
{
    public CourierFlowIntegrationTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task PickupOrder_ShouldUpdateDatabaseStatus_AndPublishOutboxMessage()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange
        var currentCourierId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var testCourier = new Courier(currentCourierId, "Kiana Driver", VehicleType.Motorcycle);
        testCourier.ToggleAvailability();

        var testOrder = new CustomerOrder(
            customerUserId: Guid.NewGuid(),
            restaurantId: Guid.NewGuid(),
            restaurantName: "Burger King",
            customerAddress: "Berlin Center",
            customerLatitude: 52.5300,
            customerLongitude: 13.4100
        );

        testOrder.AddOrderItem(quantity: 2, unitPrice: 75m, menuItemId: Guid.NewGuid(), menuItemName: "Whopper Menu");

        testOrder.MarkPaymentAsSucceeded();
        testOrder.StartPreparing();
        testOrder.TransitionToReadyForPickup();
        testOrder.AssignCourier(currentCourierId);

        var orderId = testOrder.Id;

        using (var setupScope = Factory.Services.CreateScope())
        {
            var dbContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Couriers.Add(testCourier);
            dbContext.CustomerOrders.Add(testOrder);
            await dbContext.SaveChangesAsync(ct);
        }

        var harness = Factory.Services.GetRequiredService<ITestHarness>();

        // Act 
        Client.DefaultRequestHeaders.Remove("X-Test-UserId");

        Client.DefaultRequestHeaders.Add("X-Test-UserId", currentCourierId.ToString());

        var response = await Client.PostAsync($"/api/v1/couriers/orders/{orderId}/pickup", null, ct);

        // Assert 
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var assertScope = Factory.Services.CreateScope())
        {
            var dbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedOrder = await dbContext.CustomerOrders
                .FirstOrDefaultAsync(x => x.Id == orderId, ct);
            updatedOrder.Should().NotBeNull();
            updatedOrder!.Status.Should().Be(OrderStatus.OutForDelivery,
                "The domain logic should transition the order state to OutForDelivery after pickup.");
        }

        var eventPublished = await harness.Published.Any<OrderPickedUpIntegrationEvent>(
            e => e.Context.Message.OrderId == orderId, ct);

        eventPublished.Should().BeTrue("The Outbox pattern must intercept and publish the OrderPickedUpIntegrationEvent.");
    }

    [Fact]
    public async Task PickupOrder_ShouldReturnBadRequest_WhenCourierIsNotTheAssignedOne()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange 
        var assignedCourierId = Guid.NewGuid();
        var strangerCourierId = Guid.NewGuid();

        var assignedCourier = new Courier(assignedCourierId, "Assigned Driver", VehicleType.Bicycle);
        assignedCourier.ToggleAvailability();

        var strangerCourier = new Courier(strangerCourierId, "Stranger Driver", VehicleType.Motorcycle);
        strangerCourier.ToggleAvailability();

        var testOrder = new CustomerOrder(
            customerUserId: Guid.NewGuid(),
            restaurantId: Guid.NewGuid(),
            restaurantName: "Burger King",
            customerAddress: "Berlin Center",
            customerLatitude: 52.5300,
            customerLongitude: 13.4100
        );

        testOrder.AddOrderItem(quantity: 2, unitPrice: 75m, menuItemId: Guid.NewGuid(), menuItemName: "Whopper Menu");
        testOrder.MarkPaymentAsSucceeded();
        testOrder.StartPreparing();
        testOrder.TransitionToReadyForPickup();
        testOrder.AssignCourier(assignedCourierId);

        var orderId = testOrder.Id;

        using (var setupScope = Factory.Services.CreateScope())
        {
            var dbContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Couriers.AddRange(assignedCourier, strangerCourier);
            dbContext.CustomerOrders.Add(testOrder);
            await dbContext.SaveChangesAsync(ct);
        }

        var harness = Factory.Services.GetRequiredService<ITestHarness>();

        // Act 
        Client.DefaultRequestHeaders.Remove("X-Test-UserId");
        Client.DefaultRequestHeaders.Add("X-Test-UserId", strangerCourierId.ToString());
        HttpResponseMessage? response = null;

        response = await Client.PostAsync($"/api/v1/couriers/orders/{orderId}/pickup", null, cancellationToken: ct);

        // Assert 
        response!.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "The API must reject the request because the courier identities do not match.");

        using (var assertScope = Factory.Services.CreateScope())
        {
            var dbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var currentOrderInDb = await dbContext.CustomerOrders
                .FirstOrDefaultAsync(x => x.Id == orderId, ct);

            currentOrderInDb.Should().NotBeNull();

            currentOrderInDb!.Status.Should().Be(OrderStatus.ReadyForPickup,
                "The order status in SQL Server must remain unchanged after a failed stranger pickup attempt.");

            currentOrderInDb.CourierUserId.Should().Be(assignedCourierId,
                "The courier assignment must not be overwritten by the stranger.");
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        var eventPublished = await harness.Published.Any<OrderPickedUpIntegrationEvent>(
            e => e.Context.Message.OrderId == orderId,
            cts.Token);

        eventPublished.Should().BeFalse("The system must NOT publish an integration event for an illegal pickup operation.");
    }

    [Fact]
    public async Task DeliverOrder_ShouldUpdateDatabaseStatus()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange 
        var currentCourierId = Guid.NewGuid();

        var testCourier = new Courier(currentCourierId, "Kiana Driver", VehicleType.Scooter);
        testCourier.ToggleAvailability();

        var testOrder = new CustomerOrder(
            customerUserId: Guid.NewGuid(),
            restaurantId: Guid.NewGuid(),
            restaurantName: "Pizza Hut",
            customerAddress: "Berlin Alexanderplatz",
            customerLatitude: 52.5200,
            customerLongitude: 13.4050
        );

        testOrder.AddOrderItem(quantity: 1, unitPrice: 120m, menuItemId: Guid.NewGuid(), menuItemName: "Family Pizza");

        testOrder.MarkPaymentAsSucceeded();
        testOrder.StartPreparing();
        testOrder.TransitionToReadyForPickup();
        testOrder.AssignCourier(currentCourierId);
        testOrder.TransitionToOutForDelivery(currentCourierId);

        var orderId = testOrder.Id;

        using (var setupScope = Factory.Services.CreateScope())
        {
            var dbContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Couriers.Add(testCourier);
            dbContext.CustomerOrders.Add(testOrder);
            await dbContext.SaveChangesAsync(ct);
        }

        // Act 
        Client.DefaultRequestHeaders.Remove("X-Test-UserId");
        Client.DefaultRequestHeaders.Add("X-Test-UserId", currentCourierId.ToString());
        var response = await Client.PostAsync($"/api/v1/couriers/orders/{orderId}/complete", null, cancellationToken: ct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var assertScope = Factory.Services.CreateScope())
        {
            var dbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedOrder = await dbContext.CustomerOrders
                .FirstOrDefaultAsync(x => x.Id == orderId, ct);

            updatedOrder.Should().NotBeNull();
            updatedOrder!.Status.Should().Be(OrderStatus.Delivered,
                "The domain logic must transition the order state to Delivered after a successful drop-off.");

            updatedOrder.CourierUserId.Should().Be(currentCourierId,
                "The assigned courier must remain the owner of the finalized delivery.");
        }
    }

    [Fact]
    public async Task AcceptJob_ShouldHandleConcurrency_WhenTwoCouriersTryToAcceptSimultaneously()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange 
        var courierAId = Guid.NewGuid();
        var courierBId = Guid.NewGuid();

        var courierA = new Courier(courierAId, "Courier Ali", VehicleType.Motorcycle);
        courierA.ToggleAvailability();

        var courierB = new Courier(courierBId, "Courier Reza", VehicleType.Bicycle);
        courierB.ToggleAvailability();

        var testOrder = new CustomerOrder(
            customerUserId: Guid.NewGuid(),
            restaurantId: Guid.NewGuid(),
            restaurantName: "Chipotle",
            customerAddress: "Berlin Alexanderplatz",
            customerLatitude: 52.5200,
            customerLongitude: 13.4050
        );

        testOrder.AddOrderItem(quantity: 1, unitPrice: 50m, menuItemId: Guid.NewGuid(), menuItemName: "Burrito");
        testOrder.MarkPaymentAsSucceeded();
        testOrder.StartPreparing();
        testOrder.TransitionToReadyForPickup();

        var orderId = testOrder.Id;

        using (var setupScope = Factory.Services.CreateScope())
        {
            var dbContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Couriers.AddRange(courierA, courierB);
            dbContext.CustomerOrders.Add(testOrder);
            await dbContext.SaveChangesAsync(ct);
        }

        var clientA = Factory.CreateClient();
        foreach (var header in Client.DefaultRequestHeaders)
        {
            if (header.Key != "X-Test-UserId")
                clientA.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
        clientA.DefaultRequestHeaders.Add("X-Test-UserId", courierAId.ToString());

        var clientB = Factory.CreateClient();
        foreach (var header in Client.DefaultRequestHeaders)
        {
            if (header.Key != "X-Test-UserId")
                clientB.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
        clientB.DefaultRequestHeaders.Add("X-Test-UserId", courierBId.ToString());

        // Act 
        var taskA = clientA.PostAsync($"/api/v1/couriers/orders/{orderId}/accept", null, cancellationToken: ct);
        var taskB = clientB.PostAsync($"/api/v1/couriers/orders/{orderId}/accept", null, cancellationToken: ct);

        var responses = await Task.WhenAll(taskA, taskB);
        var responseA = responses[0];
        var responseB = responses[1];
        var debugContentA = await responseA.Content.ReadAsStringAsync(ct);
        var debugContentB = await responseB.Content.ReadAsStringAsync(ct);
        // Assert 
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.NoContent);
        successCount.Should().Be(1, "Exactly one courier must successfully claim the order.");

        var failureCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest || r.StatusCode == HttpStatusCode.Conflict);
        failureCount.Should().Be(1, "The losing courier request must be rejected with an error status.");

        using (var assertScope = Factory.Services.CreateScope())
        {
            var dbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var finalizedOrder = await dbContext.CustomerOrders
                .FirstOrDefaultAsync(x => x.Id == orderId, ct);

            finalizedOrder.Should().NotBeNull();
            new[] { courierAId, courierBId }.Should().Contain(finalizedOrder!.CourierUserId);
            finalizedOrder.CourierUserId.Should().NotBe(Guid.Empty, "The order must belong to one of the active racing couriers.");
        }
    }
}
