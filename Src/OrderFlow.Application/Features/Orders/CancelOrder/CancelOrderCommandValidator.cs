using FluentValidation;

namespace OrderFlow.Application.Features.Orders.CancelOrder
{
    public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
    {
        public CancelOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty();
        }
    }
}
