using FluentValidation;

namespace OrderFlow.Application.Features.MenuItems.ChangeMenuItemPrice
{
    public class ChangeMenuItemPriceCommandValidator
        : AbstractValidator<ChangeMenuItemPriceCommand>
    {
        public ChangeMenuItemPriceCommandValidator()
        {
            RuleFor(x => x.RestaurantId).NotEmpty();
            RuleFor(x => x.MenuItemId).NotEmpty();
            RuleFor(x => x.Price).GreaterThan(0);
        }
    }
}
