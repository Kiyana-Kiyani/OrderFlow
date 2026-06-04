using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Features.Orders.PlaceOrder;
using OrderFlow.Contracts.IntegrationEvents;
using OrderFlow.Domain.Enums;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.UnitTests.Features.Orders
{
    public class PlaceOrderCommandHandlerTests : IAsyncDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<ICurrentUser> _currentUserMock;
        private readonly Mock<ILogger<PlaceOrderCommandHandler>> _loggerMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly PlaceOrderCommandHandler _handler;

        public PlaceOrderCommandHandlerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"OrderFlow_PlaceOrder_Strict_{Guid.NewGuid()}")
                .Options;

            _dbContext = new ApplicationDbContext(options);

            _currentUserMock = new Mock<ICurrentUser>();

            _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());

            _loggerMock = new Mock<ILogger<PlaceOrderCommandHandler>>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();

            _handler = new PlaceOrderCommandHandler(
                _dbContext,
                _currentUserMock.Object,
                _loggerMock.Object,
                _publishEndpointMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateOrderSuccessfully_WhenDataIsValid()
        {
            var ct = TestContext.Current.CancellationToken;

            // Arrange
            var customerId = _currentUserMock.Object.UserId;

            var restaurant = new OrderFlow.Domain.Entities.Restaurant(
                ownerUserId: Guid.NewGuid(),
                name: "Shandiz Restaurant",
                address: "Berlin Center",
                latitude: 52.5200,
                longitude: 13.4050,
                description: "Authentic Persian Food"
            );

            restaurant.AddMenuItem("Chelo Kabab", 22.0m, "With premium saffron rice");
            restaurant.AddMenuItem("Zeytoon Parvardeh", 6.5m, "Pomegranate and walnuts");

            _dbContext.Restaurants.Add(restaurant);
            await _dbContext.SaveChangesAsync(ct);

            var savedItems = await _dbContext.MenuItems.Where(x => x.RestaurantId == restaurant.Id).ToListAsync(ct);
            var item1Id = savedItems.First(x => x.Name == "Chelo Kabab").Id;
            var item2Id = savedItems.First(x => x.Name == "Zeytoon Parvardeh").Id;

            var command = new PlaceOrderCommand(
                RestaurantId: restaurant.Id,
                Items: new List<PlaceOrderItemCommand>
                {
                    new PlaceOrderItemCommand(item1Id, 2),
                    new PlaceOrderItemCommand(item2Id, 1)
                },
                CustomerAddress: "Berlin Alexanderplatz",
                CustomerLatitude: 52.5215,
                CustomerLongitude: 13.4060
            );

            _publishEndpointMock
                .Setup(x => x.Publish(It.IsAny<OrderPlacedIntegrationEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, ct);

            var savedOrder = await _dbContext.CustomerOrders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == result.OrderId, ct);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(OrderStatus.Created);
            result.TotalAmount.Should().Be(50.5m);

            savedOrder.Should().NotBeNull();
            savedOrder!.RestaurantId.Should().Be(restaurant.Id);
            savedOrder.CustomerUserId.Should().Be(customerId);
            savedOrder.RestaurantName.Should().Be("Shandiz Restaurant");
            savedOrder.OrderItems.Should().HaveCount(2);

            _publishEndpointMock.Verify(
                x => x.Publish(
                    It.Is<OrderPlacedIntegrationEvent>(e => e.OrderId == result.OrderId && e.TotalAmount == 50.5m),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRestaurantDoesNotExist_ShouldThrowNotFoundException()
        {
            var ct = TestContext.Current.CancellationToken;

            // Arrange
            var command = new PlaceOrderCommand(
                RestaurantId: Guid.NewGuid(),
                Items: new List<PlaceOrderItemCommand>(),
                CustomerAddress: "Berlin",
                CustomerLatitude: 52.5200,
                CustomerLongitude: 13.4050
            );

            // Act
            Func<Task> act = () => _handler.Handle(command, ct);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_WhenRestaurantIsDeactive_ShouldThrowNotFoundException()
        {
            var ct = TestContext.Current.CancellationToken;

            // Arrange
            var restaurant = new OrderFlow.Domain.Entities.Restaurant(Guid.NewGuid(), "Alborz", "Berlin", 52.0, 13.0);
            restaurant.Deactivate();

            _dbContext.Restaurants.Add(restaurant);
            await _dbContext.SaveChangesAsync(ct);

            var command = new PlaceOrderCommand(
                RestaurantId: restaurant.Id,
                Items: new List<PlaceOrderItemCommand>(),
                CustomerAddress: "Berlin",
                CustomerLatitude: 52.0,
                CustomerLongitude: 13.0
            );

            // Act
            Func<Task> act = () => _handler.Handle(command, ct);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_WhenMenuItemDoesNotExist_ShouldThrowNotFoundException()
        {
            var ct = TestContext.Current.CancellationToken;

            // Arrange
            var restaurant = new OrderFlow.Domain.Entities.Restaurant(Guid.NewGuid(), "Shandiz", "Berlin", 52.0, 13.0);
            _dbContext.Restaurants.Add(restaurant);
            await _dbContext.SaveChangesAsync(ct);

            var command = new PlaceOrderCommand(
                RestaurantId: restaurant.Id,
                Items: new List<PlaceOrderItemCommand>
                {
                    new PlaceOrderItemCommand(Guid.NewGuid(), 1)
                },
                CustomerAddress: "Berlin",
                CustomerLatitude: 52.0,
                CustomerLongitude: 13.0
            );

            // Act
            Func<Task> act = () => _handler.Handle(command, ct);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_WhenMenuItemIsNotAvailable_ShouldThrowConflictException()
        {
            var ct = TestContext.Current.CancellationToken;

            // Arrange
            var restaurant = new OrderFlow.Domain.Entities.Restaurant(Guid.NewGuid(), "Shandiz", "Berlin", 52.0, 13.0);
            restaurant.AddMenuItem("Ghormeh Sabzi", 18.0m);

            _dbContext.Restaurants.Add(restaurant);
            await _dbContext.SaveChangesAsync(CancellationToken.None);

            var savedItem = await _dbContext.MenuItems.FirstAsync(x => x.Name == "Ghormeh Sabzi", ct);
            restaurant.MarkMenuItemUnavailable(savedItem.Id);

            await _dbContext.SaveChangesAsync(ct);

            var command = new PlaceOrderCommand(
                RestaurantId: restaurant.Id,
                Items: new List<PlaceOrderItemCommand>
                {
                    new PlaceOrderItemCommand(savedItem.Id, 1)
                },
                CustomerAddress: "Berlin",
                CustomerLatitude: 52.0,
                CustomerLongitude: 13.0
            );

            // Act
            Func<Task> act = () => _handler.Handle(command, ct);

            // Assert
            await act.Should().ThrowAsync<ConflictException>();
        }

        public async ValueTask DisposeAsync()
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.DisposeAsync();
        }
    }
}