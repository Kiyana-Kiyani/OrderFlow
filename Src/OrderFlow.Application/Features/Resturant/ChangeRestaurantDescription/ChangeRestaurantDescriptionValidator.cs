using FluentValidation;

namespace OrderFlow.Application.Features.Resturant.ChangeRestaurantDescription
{
    public class ChangeRestaurantDescriptionValidator : AbstractValidator<ChangeRestaurantDescriptionCommand>
    {
        public ChangeRestaurantDescriptionValidator()
        {
            RuleFor(x => x.RestaurantId).NotEmpty().WithMessage("Restaurant Id is Required");

            RuleFor(x => x.NewDescription).MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.");
        }
    }
}