using FluentValidation;

namespace OrderFlow.Application.Features.Restaurant.ChangeRestaurantAddress
{
    public class ChangeRestaurantAddressValidator : AbstractValidator<ChangeRestaurantAddressCommand>
    {
        public ChangeRestaurantAddressValidator()
        {
            RuleFor(x => x.RestaurantId).NotEmpty().WithMessage("Restaurant Id is Required");
            RuleFor(x => x.NewAddress).NotEmpty().MaximumLength(200).WithMessage("Address cannot exceed 200 characters.");
        }
    }
}
