using FluentValidation;

namespace OrderFlow.Application.Features.Couriers.AcceptDelivery;

public class AcceptDeliveryJobCommandValidator : AbstractValidator<AcceptDeliveryJobCommand>
{
    public AcceptDeliveryJobCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");
    }
}