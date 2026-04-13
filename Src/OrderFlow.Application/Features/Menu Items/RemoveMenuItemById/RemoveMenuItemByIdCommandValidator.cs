using FluentValidation;

namespace OrderFlow.Application.Features.Menu_Items.RemoveMenuItemById;

public class RemoveMenuItemByIdCommandValidator
    : AbstractValidator<RemoveMenuItemByIdCommand>
{
    public RemoveMenuItemByIdCommandValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.MenuItemId).NotEmpty();
    }
}