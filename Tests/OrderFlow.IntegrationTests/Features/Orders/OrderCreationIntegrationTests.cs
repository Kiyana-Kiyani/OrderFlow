using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.IntegrationTests.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace OrderFlow.IntegrationTests.Features.Orders;

public class OrderCreationIntegrationTests : BaseIntegrationTest
{
    public OrderCreationIntegrationTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateOrder_ShouldSaveOrderInDatabase_WithCreatedAndPendingStatus()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange 
        var currentCustomerId = Guid.NewGuid();
        var burgerItemId = Guid.NewGuid();
        var pieItemId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();

        var testRestaurant = new Restaurant
        (
            ownerUserId: ownerUserId,
            name: "McDonalds",
            address: "Berlin Central Station",
            latitude: 52.5251,
            longitude: 13.3694,
            description: "Famous fast food restaurant"
        );

        testRestaurant.AddMenuItem("Big Mac Menu", 45m, "Includes a Big Mac, fries, and a drink");
        testRestaurant.AddMenuItem("Apple Pie", 10m, "Delicious apple pie with cinnamon");

        using (var setupScope = Factory.Services.CreateScope())
        {
            var dbContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Restaurants.Add(testRestaurant);
            await dbContext.SaveChangesAsync(ct);
        }

        var createOrderCommand = new
        {
            RestaurantId = testRestaurant.Id,
            CustomerAddress = "Berlin Alexanderplatz",
            CustomerLatitude = 52.5200,
            CustomerLongitude = 13.4050,
            Items = new[]
            {
                new { MenuItemId = testRestaurant.MenuItems.First().Id, Quantity = 2 },
                new { MenuItemId = testRestaurant.MenuItems.Last().Id, Quantity = 1 }
            }
        };

        Client.DefaultRequestHeaders.Remove("X-Test-UserId");
        Client.DefaultRequestHeaders.Add("X-Test-UserId", currentCustomerId.ToString());

        // Act 
        var response = await Client.PostAsJsonAsync("/api/v1/orders", createOrderCommand, ct);

        // Assert 
        IEnumerable<HttpStatusCode> codes = new[] { HttpStatusCode.Created, HttpStatusCode.OK };

        response.StatusCode.Should().BeOneOf(codes, "The API should accept valid order structures from authenticated customers.");

        using (var assertScope = Factory.Services.CreateScope())
        {
            var dbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var savedOrder = await dbContext.CustomerOrders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.CustomerUserId == currentCustomerId, ct);

            savedOrder.Should().NotBeNull("The order must be successfully persisted in the SQL database.");

            savedOrder!.RestaurantId.Should().Be(testRestaurant.Id);

            savedOrder.Status.Should().Be(OrderStatus.Created,
                "A brand new order lifecycle must always begin with 'Created' status.");

            savedOrder.Payment.Should().Be(PaymentStatus.Pending,
                "Initial order creation requires payment verification, so status must be 'Pending'.");
            savedOrder.OrderItems.Should().HaveCount(2, "The order items collection must match the request payload.");
            savedOrder.TotalAmount.Should().Be(100m, "The domain logic should auto-calculate the total sum (2 * 45 + 1 * 10 = 100).");
        }
    }
}