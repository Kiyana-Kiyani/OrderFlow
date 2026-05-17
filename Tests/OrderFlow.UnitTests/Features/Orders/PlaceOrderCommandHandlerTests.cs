//using FluentAssertions;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Moq;
//using OrderFlow.Application.Abstractions.Authentication;
//using OrderFlow.Application.Abstractions.Messaging;
//using OrderFlow.Application.Common.Exceptions;
//using OrderFlow.Application.Features.Orders.PlaceOrder;
//using OrderFlow.Domain.Entities;
//using OrderFlow.Infrastructure.Persistence;

//namespace OrderFlow.UnitTests.Features.Orders
//{
//    public class PlaceOrderCommandHandlerTests : IAsyncDisposable
//    {
//        private readonly ApplicationDbContext _dbContext;
//        private readonly Mock<ICurrentUser> _currentUser;
//        private readonly Mock<ILogger<PlaceOrderCommandHandler>> _logger;
//        private readonly Mock<IEventPublisher> _eventPublisher;

//        private readonly PlaceOrderCommandHandler _handler;

//        public PlaceOrderCommandHandlerTests()
//        {
//            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
//                .UseInMemoryDatabase(Guid.NewGuid().ToString())
//                .Options;
//            _dbContext = new ApplicationDbContext(options);

//            _currentUser = new Mock<ICurrentUser>();
//            _logger = new Mock<ILogger<PlaceOrderCommandHandler>>();
//            _eventPublisher = new Mock<IEventPublisher>();

//            _handler = new PlaceOrderCommandHandler(_dbContext, _currentUser.Object, _logger.Object, _eventPublisher.Object);
//        }

//        [Fact]
//        public async Task Handle_ShouldCreateOrderSuccessfully_WhenDataIsValid()
//        {
//            //arrange
//            var currentUserGuid = Guid.NewGuid();
//            _currentUser.Setup(x => x.UserId).Returns(currentUserGuid);

//            var restaurant = new Restaurant(currentUserGuid, "spring", "Address of Restaurant");

//            var itemGuid1 = restaurant.AddMenuItem("Item 1", 10m);
//            var itemGuid2 = restaurant.AddMenuItem("Item 2", 20m);

//            _dbContext.Restaurants.Add(restaurant);
//            await _dbContext.SaveChangesAsync(CancellationToken.None);

//            var command = new PlaceOrderCommand(RestaurantId: restaurant.Id, new List<PlaceOrderItemCommand>
//                {
//                    new PlaceOrderItemCommand(itemGuid1, 1),
//                    new PlaceOrderItemCommand(itemGuid2, 2),
//                });

//            //act
//            var result = await _handler.Handle(command, CancellationToken.None);

//            var savedOrder = await _dbContext.CustomerOrders
//                .FirstOrDefaultAsync(o => o.Id == result.OrderId, CancellationToken.None);

//            //assert
//            result.Should().NotBeNull();
//            result.TotalAmount.Should().Be(50m);
//            result.OrderId.Should().NotBeEmpty();

//            savedOrder.Should().NotBeNull();
//            savedOrder!.RestaurantId.Should().Be(restaurant.Id);
//            savedOrder!.CustomerUserId.Should().Be(currentUserGuid);


//            _logger.Verify(x => x.Log(
//                LogLevel.Information,
//                It.IsAny<EventId>(),
//                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("placed successfully")),
//                null,
//                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
//        }


//        [Fact]
//        public async Task Handle_WhenRestaurantDoesNotExist_ShouldThrowNotFoundException()
//        {
//            //Arrange
//            var command = new PlaceOrderCommand(RestaurantId: Guid.NewGuid(), new List<PlaceOrderItemCommand>());

//            //Act
//            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

//            //assert
//            await act.Should().ThrowAsync<NotFoundException>();
//        }
//        [Fact]
//        public async Task Handle_WhenRestaurantIsDeactive_ShouldThrowNotFoundException()
//        {
//            //Arrange
//            var command = new PlaceOrderCommand(RestaurantId: Guid.NewGuid(), new List<PlaceOrderItemCommand>());
//            var restaurant = new Restaurant(Guid.NewGuid(), "NameTest", "Test Address");
//            restaurant.Deactivate();
//            _dbContext.Restaurants.Add(restaurant);
//            await _dbContext.SaveChangesAsync(CancellationToken.None);

//            //Act
//            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

//            //Assert
//            await act.Should().ThrowAsync<NotFoundException>();
//        }

//        [Fact]
//        public async Task Handle_WhenMenuItemDoesNotExist_ShouldThrowNotFoundException()
//        {
//            //Arrange
//            var command = new PlaceOrderCommand(RestaurantId: Guid.NewGuid(), new List<PlaceOrderItemCommand>
//                {
//                    new PlaceOrderItemCommand(Guid.NewGuid(), 1)
//                });

//            //Act
//            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);
//            //Assert
//            await act.Should().ThrowAsync<NotFoundException>();
//        }

//        [Fact]
//        public async Task Handle_WhenMenuItemIsNotAvailable_ShouldThrowConflictException()
//        {
//            //Arrange
//            var restaurantGuid = Guid.NewGuid();
//            var restaurant = new Restaurant(restaurantGuid, "NameTest", "Test Address");
//            var itemGuid = restaurant.AddMenuItem("Test Item 1", 10m);
//            restaurant.MarkMenuItemUnavailable(itemGuid);
//            _dbContext.Restaurants.Add(restaurant);
//            await _dbContext.SaveChangesAsync(CancellationToken.None);

//            var command = new PlaceOrderCommand(RestaurantId: restaurantGuid, new List<PlaceOrderItemCommand>
//                {
//                    new PlaceOrderItemCommand(itemGuid, 1)
//                });
//            //Act
//            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);
//            //Assert
//            await act.Should().ThrowAsync<ConflictException>();
//        }

//        public async ValueTask DisposeAsync()
//        {
//            await _dbContext.Database.EnsureDeletedAsync();
//            await _dbContext.DisposeAsync();
//        }
//    }
//}