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
            // ساخت پایگاه داده حافظه‌ای مجزا با نام یکتا برای جلوگیری از تداخل استیت‌ها
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"OrderFlow_PlaceOrder_Strict_{Guid.NewGuid()}")
                .Options;

            _dbContext = new ApplicationDbContext(options);

            _currentUserMock = new Mock<ICurrentUser>();
            _loggerMock = new Mock<ILogger<PlaceOrderCommandHandler>>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();

            // پیاده‌سازی عینی هماهنگ با نیازمندی اینترفیس IApplicationDbContext
            _handler = new PlaceOrderCommandHandler(
                _dbContext,
                _currentUserMock.Object,
                _loggerMock.Object,
                _publishEndpointMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateOrderSuccessfully_WhenDataIsValid()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _currentUserMock.Setup(x => x.UserId).Returns(customerId);

            // ۱. نمونه‌سازی دقیق رستوران بر اساس سازنده اصلی شما
            var restaurant = new OrderFlow.Domain.Entities.Restaurant(
                ownerUserId: Guid.NewGuid(),
                name: "Shandiz Restaurant",
                address: "Berlin Center",
                latitude: 52.5200,
                longitude: 13.4050,
                description: "Authentic Persian Food"
            );

            // ۲. اضافه کردن منو از طریق رفتار اصیل دامین مدل رستوران شما
            restaurant.AddMenuItem("Chelo Kabab", 22.0m, "With premium saffron rice");
            restaurant.AddMenuItem("Zeytoon Parvardeh", 6.5m, "Pomegranate and walnuts");

            _dbContext.Restaurants.Add(restaurant);
            await _dbContext.SaveChangesAsync(CancellationToken.None);

            // ۳. واکشی آیتم‌ها از دیتابیس برای به دست آوردن Idهای تولید شده توسط EF Core
            var savedItems = await _dbContext.MenuItems.Where(x => x.RestaurantId == restaurant.Id).ToListAsync();
            var item1Id = savedItems.First(x => x.Name == "Chelo Kabab").Id;
            var item2Id = savedItems.First(x => x.Name == "Zeytoon Parvardeh").Id;

            var command = new PlaceOrderCommand(
                RestaurantId: restaurant.Id,
                Items: new List<PlaceOrderItemCommand>
                {
                    new PlaceOrderItemCommand(item1Id, 2), // 2 * 22.0 = 44.0
                    new PlaceOrderItemCommand(item2Id, 1)  // 1 * 6.5  = 6.5
                },
                CustomerAddress: "Berlin Alexanderplatz",
                CustomerLatitude: 52.5215,
                CustomerLongitude: 13.4060
            );

            _publishEndpointMock
                .Setup(x => x.Publish(It.IsAny<OrderPlacedIntegrationEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            var savedOrder = await _dbContext.CustomerOrders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == result.OrderId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(OrderStatus.Created);
            result.TotalAmount.Should().Be(50.5m); // 44.0 + 6.5

            savedOrder.Should().NotBeNull();
            savedOrder!.RestaurantId.Should().Be(restaurant.Id);
            savedOrder.CustomerUserId.Should().Be(customerId);
            savedOrder.RestaurantName.Should().Be("Shandiz Restaurant");
            savedOrder.OrderItems.Should().HaveCount(2);

            // صحت‌سنجی برودکاست اِونت در الگوی تفکیک پیام MassTransit
            _publishEndpointMock.Verify(
                x => x.Publish(
                    It.Is<OrderPlacedIntegrationEvent>(e => e.OrderId == result.OrderId && e.TotalAmount == 50.5m),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRestaurantDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            var command = new PlaceOrderCommand(
                RestaurantId: Guid.NewGuid(),
                Items: new List<PlaceOrderItemCommand>(),
                CustomerAddress: "Berlin",
                CustomerLatitude: 52.5200,
                CustomerLongitude: 13.4050
            );

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_WhenRestaurantIsDeactive_ShouldThrowNotFoundException()
        {
            // Arrange
            var restaurant = new OrderFlow.Domain.Entities.Restaurant(Guid.NewGuid(), "Alborz", "Berlin", 52.0, 13.0);
            restaurant.Deactivate(); // قفل کردن وضعیت رستوران در لایه دامین

            _dbContext.Restaurants.Add(restaurant);
            await _dbContext.SaveChangesAsync(CancellationToken.None);

            var command = new PlaceOrderCommand(
                RestaurantId: restaurant.Id,
                Items: new List<PlaceOrderItemCommand>(),
                CustomerAddress: "Berlin",
                CustomerLatitude: 52.0,
                CustomerLongitude: 13.0
            );

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_WhenMenuItemDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            var restaurant = new OrderFlow.Domain.Entities.Restaurant(Guid.NewGuid(), "Shandiz", "Berlin", 52.0, 13.0);
            _dbContext.Restaurants.Add(restaurant);
            await _dbContext.SaveChangesAsync(CancellationToken.None);

            var command = new PlaceOrderCommand(
                RestaurantId: restaurant.Id,
                Items: new List<PlaceOrderItemCommand>
                {
                    new PlaceOrderItemCommand(Guid.NewGuid(), 1) // استفاده از آیدی ناموجود منو
                },
                CustomerAddress: "Berlin",
                CustomerLatitude: 52.0,
                CustomerLongitude: 13.0
            );

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_WhenMenuItemIsNotAvailable_ShouldThrowConflictException()
        {
            // Arrange
            var restaurant = new OrderFlow.Domain.Entities.Restaurant(Guid.NewGuid(), "Shandiz", "Berlin", 52.0, 13.0);
            restaurant.AddMenuItem("Ghormeh Sabzi", 18.0m);

            _dbContext.Restaurants.Add(restaurant);
            await _dbContext.SaveChangesAsync(CancellationToken.None);

            // استخراج آیدی تخصیص یافته به غذا جهت غیرفعال‌سازی موجودی آن
            var savedItem = await _dbContext.MenuItems.FirstAsync(x => x.Name == "Ghormeh Sabzi");
            restaurant.MarkMenuItemUnavailable(savedItem.Id);

            await _dbContext.SaveChangesAsync(CancellationToken.None);

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
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

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