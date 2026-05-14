using FluentValidation;
using SmartLogistics.Application.DTOs.Auth;

namespace SmartLogistics.Application.Common.Validators
{
    // Validator for new user registration data in the system
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            // Full Name
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(100).WithMessage("Full name must not exceed 100 characters");

            // Email Address
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address format");

            // Password (Complexity Requirements)
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter (A-Z)")
                .Matches("[0-9]").WithMessage("Password must contain at least one number (0-9)");

            // Phone Number (International Validation)
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{6,14}$").WithMessage("Invalid phone number format (Example: +2010...)");
        }
    }
}