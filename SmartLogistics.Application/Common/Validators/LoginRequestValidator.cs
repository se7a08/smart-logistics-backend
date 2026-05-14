using FluentValidation;
using SmartLogistics.Application.DTOs.Auth;

namespace SmartLogistics.Application.Common.Validators
{
    // Validator for login request data
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            // Email validation
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address format");

            // Password validation
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}