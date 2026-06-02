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
        private readonly Mock<ILogger<CreateRestaurantCommandHandler>> _loggerMock;
        private readonly CreateRestaurantCommandHandler _handler;

        public CreateRestaurantCommandHandlerTests()
        {
            _loggerMock = new Mock<ILogger<CreateRestaurantCommandHandler>>();

            // ساخت یک دیتابیس حافظه‌ای کاملاً ایزوله با نام منحصربه‌فرد برای این تست
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"OrderFlow_CreateRestaurant_{Guid.NewGuid()}")
                .Options;

            _dbContext = new ApplicationDbContext(options);

            // کلاس فیزیکی دیتابیس به عنوان پیاده‌کننده IApplicationDbContext پاس داده می‌شود
            _handler = new CreateRestaurantCommandHandler(_dbContext, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateRestaurantSuccessfully_WhenDataIsValid()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var command = new CreateRestaurantCommand(
                Name: "Test Restaurant",
                Address: "123 Test Street",
                Description: "A test restaurant for unit testing.",
                OwnerId: ownerId,
                Latitude: 40.7128,
                Longitude: -74.0060
            );

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // واکشی داده ثبت شده از دیتابیس حافظه جهت راستی‌آزمایی
            var savedRestaurant = await _dbContext.Restaurants
                .FirstOrDefaultAsync(x => x.Id == result.RestaurantId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.RestaurantId.Should().NotBeEmpty();

            savedRestaurant.Should().NotBeNull();
            savedRestaurant!.Name.Should().Be("Test Restaurant");
            savedRestaurant.Address.Should().Be("123 Test Street");
            savedRestaurant.Description.Should().Be("A test restaurant for unit testing.");
            savedRestaurant.OwnerUserId.Should().Be(ownerId);
            savedRestaurant.Latitude.Should().Be(40.7128);
            savedRestaurant.Longitude.Should().Be(-74.0060);
            savedRestaurant.IsActive.Should().BeTrue("A newly created restaurant must be Active by default according to domain rules.");

            // بررسی دقیق و قطعی ثبت لاگ در کتابخانه Moq بدون حساسیت به فرمت‌پذیری رشته
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("created successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        public async ValueTask DisposeAsync()
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.DisposeAsync();
        }
    }
}