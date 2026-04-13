using FluentValidation;

namespace OrderFlow.Application.Features.Menu_Items.MarkMenuItemAvailable
{
    public class MarkMenuItemAvailableCommandValidator
        : AbstractValidator<MarkMenuItemAvailableCommand>
    {
        public MarkMenuItemAvailableCommandValidator()
        {
            RuleFor(x => x.RestaurantId).NotEmpty();
            RuleFor(x => x.MenuItemId).NotEmpty();
        }
    }
}
