using FluentValidation;
using SmartLogistics.Application.DTOs.Auth;

namespace SmartLogistics.Application.Common.Validators
{
    // فحص صحة بيانات إنشاء حساب جديد في النظام
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            // الاسم بالكامل
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("الاسم بالكامل مطلوب")
                .MaximumLength(100).WithMessage("الاسم لا يجب أن يتجاوز 100 حرف");

            // البريد الإلكتروني
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
                .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة");

            // كلمة المرور (شروط القوة)
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة")
                .MinimumLength(8).WithMessage("كلمة المرور يجب ألا تقل عن 8 رموز")
                .Matches("[A-Z]").WithMessage("يجب أن تحتوي كلمة المرور على حرف كبير واحد على الأقل (A-Z)")
                .Matches("[0-9]").WithMessage("يجب أن تحتوي كلمة المرور على رقم واحد على الأقل (0-9)");

            // رقم الهاتف (Validation دولي)
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("رقم الهاتف مطلوب")
                .Matches(@"^\+?[1-9]\d{6,14}$").WithMessage("صيغة رقم الهاتف غير صحيحة (مثال: +2010...)");
        }
    }
}