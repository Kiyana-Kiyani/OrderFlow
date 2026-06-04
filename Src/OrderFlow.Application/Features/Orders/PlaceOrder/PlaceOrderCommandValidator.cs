using FluentValidation;

namespace OrderFlow.Application.Features.Orders.PlaceOrder
{
    public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
    {
        public PlaceOrderCommandValidator()
        {
            RuleFor(x => x.RestaurantId).NotEmpty();

            RuleFor(x => x.Items).NotNull().NotEmpty();

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.MenuItemId)
                    .NotEmpty();

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0);
            });
        }
    }
}
