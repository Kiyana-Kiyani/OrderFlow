using FluentValidation.TestHelper;
using OrderFlow.Application.Features.Orders.PlaceOrder;

namespace OrderFlow.UnitTests.Features.Orders
{
    public class PlaceOrderCommandValidatorTests
    {
        private readonly PlaceOrderCommandValidator _validator;

        public PlaceOrderCommandValidatorTests()
        {
            _validator = new PlaceOrderCommandValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void Validate_ShouldHaveError_WhenQuantityIsNotGreaterThanZero(int quantity)
        {
            // Arrange
            var command = new PlaceOrderCommand(
                RestaurantId: Guid.NewGuid(),
                Items: new List<PlaceOrderItemCommand>
                {
                    new PlaceOrderItemCommand(Guid.NewGuid(), quantity)
                },
                CustomerAddress: "Berlin Center",
                CustomerLatitude: 52.5200,
                CustomerLongitude: 13.4050
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor("Items[0].Quantity");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void Validate_ShouldNotHaveError_WhenCommandIsValid(int quantity)
        {
            // Arrange
            var command = new PlaceOrderCommand(
                RestaurantId: Guid.NewGuid(),
                Items: new List<PlaceOrderItemCommand>
                {
                    new PlaceOrderItemCommand(Guid.NewGuid(), quantity)
                },
                CustomerAddress: "Berlin Center",
                CustomerLatitude: 52.5200,
                CustomerLongitude: 13.4050
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor("Items[0].Quantity");
        }

        [Fact]
        public void Validate_ShouldHaveError_WhenRestaurantIdIsEmpty()
        {
            // Arrange
            var command = new PlaceOrderCommand(
                RestaurantId: Guid.Empty,
                Items: new List<PlaceOrderItemCommand> { new PlaceOrderItemCommand(Guid.NewGuid(), 1) },
                CustomerAddress: "Berlin",
                CustomerLatitude: 52.5200,
                CustomerLongitude: 13.4050
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RestaurantId);
        }

        [Fact]
        public void Validate_ShouldHaveError_WhenItemsListIsEmpty()
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
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Items);
        }

        [Fact]
        public void Validate_ShouldHaveError_WhenMenuItemIdIsEmpty()
        {
            // Arrange
            var command = new PlaceOrderCommand(
                RestaurantId: Guid.NewGuid(),
                Items: new List<PlaceOrderItemCommand>
                {
                    new PlaceOrderItemCommand(Guid.Empty, 1)
                },
                CustomerAddress: "Berlin",
                CustomerLatitude: 52.5200,
                CustomerLongitude: 13.4050
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor("Items[0].MenuItemId");
        }
    }
}