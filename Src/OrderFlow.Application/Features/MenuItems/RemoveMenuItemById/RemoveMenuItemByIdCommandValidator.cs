using FluentValidation;

namespace OrderFlow.Application.Features.MenuItems.RemoveMenuItemById;

public class RemoveMenuItemByIdCommandValidator
    : AbstractValidator<RemoveMenuItemByIdCommand>
{
    public RemoveMenuItemByIdCommandValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.MenuItemId).NotEmpty();
    }
}