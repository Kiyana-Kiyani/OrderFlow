using FluentValidation;

namespace OrderFlow.Application.Features.Resturant.ChangeRestaurantName
{
    public class ChangeRestaurantNameValidator : AbstractValidator<ChangeRestaurantNameCommand>
    {
        public ChangeRestaurantNameValidator()
        {
            RuleFor(x => x.RestaurantId).NotEmpty();
            RuleFor(x => x.NewName)
                .NotEmpty().WithMessage("نام رستوران نمی‌تواند خالی باشد.")
                .MaximumLength(100).WithMessage("نام رستوران نمی‌تواند بیشتر از 100 کاراکتر باشد.");
        }
    }
}