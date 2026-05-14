using FluentValidation;
using SmartLogistics.Application.DTOs.Warehouses;

namespace SmartLogistics.Application.Common.Validators
{
    // فحص صحة بيانات إضافة مخزن/مستودع جديد للسيستم
    public class CreateWarehouseRequestValidator : AbstractValidator<CreateWarehouseRequest>
    {
        public CreateWarehouseRequestValidator()
        {
            // التحقق من هوية المخزن
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم المخزن مطلوب")
                .MaximumLength(200).WithMessage("الاسم طويل جداً (أقصى حد 200 حرف)");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("كود المخزن مطلوب")
                .MaximumLength(20).WithMessage("الكود لا يجب أن يتخطى 20 حرف")
                .Matches("^[A-Z0-9-]+$").WithMessage("الكود يجب أن يحتوي على حروف كبيرة وأرقام وعلامة (-) فقط");

            // الموقع الجغرافي
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("عنوان المخزن بالتفصيل مطلوب");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("المحافظة/المدينة مطلوبة");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("إحداثيات العرض غير صحيحة");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("إحداثيات الطول غير صحيحة");

            // السعة التشغيلية
            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("سعة المخزن يجب أن تكون أكبر من صفر");

            // بيانات المسؤول عن المخزن
            RuleFor(x => x.ManagerName)
                .NotEmpty().WithMessage("اسم مدير المخزن مطلوب");

            RuleFor(x => x.ManagerPhone)
                .NotEmpty().WithMessage("رقم تليفون المدير مطلوب");
        }
    }
}