using FluentValidation;

namespace OrderFlow.Application.Features.Couriers.PickupOrder;

public class PickupOrderCommandValidator : AbstractValidator<PickupOrderCommand>
{
    public PickupOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");
    }
}