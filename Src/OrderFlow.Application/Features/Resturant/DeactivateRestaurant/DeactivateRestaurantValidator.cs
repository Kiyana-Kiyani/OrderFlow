using FluentValidation;

namespace OrderFlow.Application.Features.Resturant.DeactivateRestaurant
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