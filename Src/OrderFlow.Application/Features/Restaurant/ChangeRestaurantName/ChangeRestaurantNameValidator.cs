using FluentValidation;

namespace OrderFlow.Application.Features.Restaurant.ChangeRestaurantName
{
    public class ChangeRestaurantNameValidator : AbstractValidator<ChangeRestaurantNameCommand>
    {
        public ChangeRestaurantNameValidator()
        {
            RuleFor(x => x.RestaurantId).NotEmpty().WithMessage("Restaurant Id is Required");
            RuleFor(x => x.NewName)
                .NotEmpty().WithMessage("Resturant Name is required.")
                .MaximumLength(100).WithMessage("Restaurant name cannot exceed 100 characters.");
        }
    }
}