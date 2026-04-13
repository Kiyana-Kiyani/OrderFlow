using FluentValidation;

namespace OrderFlow.Application.Features.Resturant.ChangeRestaurantAddress
{
    public class ChangeRestaurantAddressValidator : AbstractValidator<ChangeRestaurantAddressCommand>
    {
        public ChangeRestaurantAddressValidator()
        {
            RuleFor(x => x.RestaurantId).NotEmpty();
            RuleFor(x => x.NewAddress).NotEmpty().MaximumLength(200);
        }
    }
}
