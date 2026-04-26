using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Features.Orders.PlaceOrder;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Features.Orders
{
    public class PlaceOrderCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _dbContext;
        private readonly Mock<ICurrentUser> _currentUser;
        private readonly Mock<ILogger<PlaceOrderCommandHandler>> _logger;

        private readonly PlaceOrderCommandHandler _handler;

        public PlaceOrderCommandHandlerTests()
        {
            _dbContext = new Mock<IApplicationDbContext>();
            _currentUser = new Mock<ICurrentUser>();
            _logger = new Mock<ILogger<PlaceOrderCommandHandler>>();

            _handler = new PlaceOrderCommandHandler(_dbContext.Object, _currentUser.Object, _logger.Object);
        }

        [Fact]
        public async Task Handle_WhenRestaurantDoesNotExist_ShouldThrowNotFoundException()
        {
            //Arrange
            var command = new PlaceOrderCommand(RestaurantId: Guid.NewGuid(), new List<PlaceOrderItemCommand>());
            var restaurantsMock = new List<Restaurant>().AsQueryable().BuildMockDbSet

            //Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            //assert
            await act.Should().ThrowAsync<NotFoundException>();

        }
    }
}
