using FluentValidation;

namespace OrderFlow.Application.Features.Couriers.CompleteDelivery
{
    public class CompleteDeliveryCommandValidator : AbstractValidator<CompleteDeliveryCommand>
    {
        public CompleteDeliveryCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("OrderId is required.");
        }
    }
}