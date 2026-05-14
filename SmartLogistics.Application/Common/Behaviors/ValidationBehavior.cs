using FluentValidation;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValidationException = SmartLogistics.Application.Common.Exceptions.ValidationException;

namespace SmartLogistics.Application.Common.Behaviors
{
    // ميدل وير عشان يشغل الـ FluentValidation تلقائياً قبل ما الـ Request يوصل للـ Handler
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // لو مفيش أي شروط (Validators) متعرفة للطلب ده، كمل عادي
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);
            var failures = new List<FluentValidation.Results.ValidationFailure>();

            // بنمشي على كل الـ Validators ونجمع الأخطاء
            foreach (var validator in _validators)
            {
                var result = await validator.ValidateAsync(context, cancellationToken);
                if (!result.IsValid)
                {
                    failures.AddRange(result.Errors);
                }
            }

            // لو فيه أخطاء، بنجمعهم ونرمي الـ Exception بتاعنا
            if (failures.Count > 0)
            {
                // تحويل الأخطاء لشكل Dictionary (اسم الحقل : مجموعة الأخطاء اللي فيه)
                var errorsDictionary = new Dictionary<string, string[]>();

                var propertyGroups = failures
                    .GroupBy(x => x.PropertyName)
                    .Select(g => new { PropertyName = g.Key, Messages = g.Select(x => x.ErrorMessage).ToArray() });

                foreach (var group in propertyGroups)
                {
                    errorsDictionary.Add(group.PropertyName, group.Messages);
                }

                throw new ValidationException(errorsDictionary);
            }

            return await next();
        }
    }
}