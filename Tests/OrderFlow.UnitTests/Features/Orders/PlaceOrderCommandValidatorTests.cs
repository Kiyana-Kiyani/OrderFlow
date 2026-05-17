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
        [InlineData(-1)]
        public void Validate_ShouldHaveError_WhenQuantityIsNotGreaterThanZero(int quantity)
        {
            //arrange
            var command = new PlaceOrderCommand(Guid.NewGuid(), new List<PlaceOrderItemCommand>
                        {
                            new PlaceOrderItemCommand(Guid.NewGuid(), quantity),
                        });
            //act
            var result = _validator.TestValidate(command);

            //assert
            result.ShouldHaveValidationErrorFor(c => c.Items[0].Quantity);

        }

        [Theory]
        [InlineData(1)]
        [InlineData(20)]
        public void Validate_ShouldNotHaveError_WhenCommandIsValid(int quantity)
        {
            //arrange
            var command = new PlaceOrderCommand(Guid.NewGuid(), new List<PlaceOrderItemCommand>
                        {
                            new PlaceOrderItemCommand(Guid.NewGuid(), quantity),
                        });
            //act
            var result = _validator.TestValidate(command);

            //assert
            result.ShouldNotHaveValidationErrorFor(c => c.Items[0].Quantity);
        }

        [Fact]
        public void Should_Have_Error_When_RestaurantId_Is_Empty()
        {
            // Arrange
            var command = new PlaceOrderCommand(RestaurantId: Guid.Empty, Items: new List<PlaceOrderItemCommand>());

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.RestaurantId);
        }

        [Fact]
        public void Should_Have_Error_When_Items_Is_Empty()
        {
            // Arrange
            var command = new PlaceOrderCommand
            (
                RestaurantId: Guid.NewGuid(),
                Items: new List<PlaceOrderItemCommand>()
            );

            // Act & Assert
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Items);
        }

        [Fact]
        public void Should_Have_Error_When_MenuItemId_In_Items_Is_Empty()
        {
            // Arrange
            var command = new PlaceOrderCommand
            (
                RestaurantId: Guid.NewGuid(),
                Items: new List<PlaceOrderItemCommand>
            {
                new PlaceOrderItemCommand(Guid.Empty, 5)
            }
            );

            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor("Items[0].MenuItemId");
        }
    }
}
