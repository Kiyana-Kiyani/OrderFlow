using FluentValidation;

namespace OrderFlow.Application.Features.Couriers.UpdateProfile;

public class UpdateCourierProfileCommandValidator : AbstractValidator<UpdateCourierProfileCommand>
{
    // 🚀 اصلاح شد: تبدیل به متد سازنده استاندارد بدون کلمه کلیدی class
    public UpdateCourierProfileCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Courier name is required.")
            .MaximumLength(100).WithMessage("Courier name cannot exceed 100 characters.")
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Courier name cannot consist of spaces only.");

        RuleFor(x => x.VehicleType)
            .IsInEnum().WithMessage("The selected vehicle type is invalid. Please select a valid option (Bicycle, Scooter, Motorcycle, or Car).");
    }
}