using FluentValidation;

namespace OrderFlow.Application.Features.Restaurant.DeactivateRestaurant
{
    public class DeactivateRestaurantValidator : AbstractValidator<DeactivateRestaurantCommand>
    {
        public DeactivateRestaurantValidator()
        {
            RuleFor(x => x.RestaurantId)
                .NotEmpty().WithMessage("Restaurant Id is not valid");
        }
    }
}