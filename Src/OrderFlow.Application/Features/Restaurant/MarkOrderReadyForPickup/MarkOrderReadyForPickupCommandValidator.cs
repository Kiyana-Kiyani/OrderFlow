using FluentValidation;

namespace OrderFlow.Application.Features.Restaurant.MarkOrderReadyForPickup;

public class MarkOrderReadyForPickupCommandValidator : AbstractValidator<MarkOrderReadyForPickupCommand>
{
    public MarkOrderReadyForPickupCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");
    }
}