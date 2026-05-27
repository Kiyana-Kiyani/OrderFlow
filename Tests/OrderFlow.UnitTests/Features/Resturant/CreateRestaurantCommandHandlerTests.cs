using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderFlow.Application.Features.Restaurant.CreateRestaurant;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.UnitTests.Features.Restaurant
{
    public class CreateRestaurantCommandHandlerTests : IAsyncDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<ILogger<CreateRestaurantCommandHandler>> _logger;
        private readonly CreateRestaurantCommandHandler _handler;

        public CreateRestaurantCommandHandlerTests()
        {
            _logger = new Mock<ILogger<CreateRestaurantCommandHandler>>();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(options);
            _handler = new CreateRestaurantCommandHandler(_dbContext, _logger.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateRestaurantSuccessfully_WhenDataIsValid()
        {
            //arrange
            var ownerId = Guid.NewGuid();
            var command = new CreateRestaurantCommand(
                Name: "Test Restaurant",
                Address: "123 Test Street",
                Latitude: 40.7128,
                Longitude: -74.0060,
                Description: "A test restaurant for unit testing.",
                OwnerId: ownerId
            );
            //act
            var result = await _handler.Handle(command, CancellationToken.None);

            var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(x => x.Id == result.RestaurantId, CancellationToken.None);

            //assert
            result.Should().NotBeNull();
            result.RestaurantId.Should().NotBeEmpty();
            restaurant.Should().NotBeNull();
            restaurant.Should().BeEquivalentTo(new
            {
                Name = "Test Restaurant",
                Address = "123 Test Street",
                Description = "A test restaurant for unit testing.",
                OwnerId = ownerId
            });

            _logger.Verify(x => x.Log(

                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("created successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ), Times.Once());
        }

        public async ValueTask DisposeAsync()
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.DisposeAsync();

        }
    }
}
