using FluentValidation;
using SmartLogistics.Application.DTOs.Auth;

namespace SmartLogistics.Application.Common.Validators
{
    // فحص صحة بيانات تسجيل الدخول
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            // التحقق من البريد الإلكتروني
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
                .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة");

            // التحقق من كلمة المرور
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة");
        }
    }
}