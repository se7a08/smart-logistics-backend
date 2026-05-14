using FluentValidation;
using SmartLogistics.Application.DTOs.Shipments;

namespace SmartLogistics.Application.Common.Validators
{
    // كلاس التحقق من صحة بيانات إنشاء شحنة جديدة
    public class CreateShipmentRequestValidator : AbstractValidator<CreateShipmentRequest>
    {
        public CreateShipmentRequestValidator()
        {
            // التحقق من بيانات المستلم
            RuleFor(x => x.RecipientName)
                .NotEmpty().WithMessage("اسم المستلم مطلوب")
                .MaximumLength(100).WithMessage("الاسم لا يمكن أن يتجاوز 100 حرف");

            RuleFor(x => x.RecipientPhone)
                .NotEmpty().WithMessage("رقم موبايل المستلم مطلوب");

            RuleFor(x => x.RecipientEmail)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
                .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة");

            // التحقق من بيانات العنوان والموقع الجغرافي
            RuleFor(x => x.DeliveryAddress)
                .NotEmpty().WithMessage("عنوان التوصيل مطلوب")
                .MaximumLength(500).WithMessage("العنوان طويل جداً");

            RuleFor(x => x.DeliveryLatitude)
                .InclusiveBetween(-90, 90).WithMessage("إحداثيات العرض غير صحيحة");

            RuleFor(x => x.DeliveryLongitude)
                .InclusiveBetween(-180, 180).WithMessage("إحداثيات الطول غير صحيحة");

            // التحقق من مواصفات الشحنة
            RuleFor(x => x.Weight)
                .GreaterThan(0).WithMessage("يجب أن يكون وزن الشحنة أكبر من صفر");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("وصف الشحنة مطلوب")
                .MaximumLength(500).WithMessage("الوصف لا يتجاوز 500 حرف");

            RuleFor(x => x.DeclaredValue)
                .GreaterThanOrEqualTo(0).WithMessage("القيمة المعلنة لا يمكن أن تكون بالسالب");

            // التحقق من مسار الشحنة
            RuleFor(x => x.OriginWarehouseId)
                .NotEmpty().WithMessage("يجب تحديد مخزن القيام");

            RuleFor(x => x.DestinationWarehouseId)
                .NotEmpty().WithMessage("يجب تحديد مخزن الوصول")
                .NotEqual(x => x.OriginWarehouseId).WithMessage("عفواً، مخزن الوصول يجب أن يكون مختلفاً عن مخزن القيام");
        }
    }
}