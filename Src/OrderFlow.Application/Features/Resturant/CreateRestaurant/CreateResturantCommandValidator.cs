using FluentValidation;

namespace OrderFlow.Application.Features.Resturant.CreateRestaurant
{
    public class CreateResturantCommandValidator : AbstractValidator<CreateRestaurantCommand>
    {

        public CreateResturantCommandValidator()
        {

            RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Restaurant name is required.")
                    .MaximumLength(100).WithMessage("Restaurant name must not exceed 100 characters.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Restaurant address is required.")
                .MaximumLength(200).WithMessage("Restaurant address must not exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Restaurant description must not exceed 500 characters.");

        }
    }
}
