using FluentValidation;

namespace OrderFlow.Application.Features.MenuItems.MarkMenuItemUnavailable;

public class MarkMenuItemUnavailableCommandValidator
    : AbstractValidator<MarkMenuItemUnavailableCommand>
{
    public MarkMenuItemUnavailableCommandValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.MenuItemId).NotEmpty();
    }
}