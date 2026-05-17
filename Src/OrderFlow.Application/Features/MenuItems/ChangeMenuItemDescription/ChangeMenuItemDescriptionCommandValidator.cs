using FluentValidation;

namespace OrderFlow.Application.Features.MenuItems.ChangeMenuItemDescription
{

    public class ChangeMenuItemDescriptionCommandValidator : AbstractValidator<ChangeMenuItemDescriptionCommand>
    {
        public ChangeMenuItemDescriptionCommandValidator()
        {
            RuleFor(x => x.RestaurantId).NotEmpty();
            RuleFor(x => x.MenuItemId).NotEmpty();
            RuleFor(x => x.Description).MaximumLength(500);
        }
    }
}
