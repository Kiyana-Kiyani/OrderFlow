using FluentValidation;

namespace OrderFlow.Application.Features.MenuItems.AddMenuItem
{
    public class AddMenuItemCommandValidator : AbstractValidator<AddMenuItemCommand>
    {
        public AddMenuItemCommandValidator()
        {
            RuleFor(x => x.RestaurantId).NotEmpty();

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Price)
                .GreaterThan(0);

            RuleFor(x => x.Description)
                .MaximumLength(500);
        }
    }
}
