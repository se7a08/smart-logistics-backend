using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationException = SmartLogistics.Application.Common.Exceptions.ValidationException;
namespace SmartLogistics.Application.Common.Behaviors
{   /// <summary>
    /// MediatR pipeline behavior that runs FluentValidation before any command/query handler.
    /// Automatically validates all requests that have registered validators.
    /// </summary>
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
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                var errors = failures
                    .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                    .ToDictionary(g => g.Key, g => g.ToArray());

                throw new ValidationException(errors);
            }

            return await next();
        }
    }

    /// <summary>
    /// MediatR pipeline behavior for performance logging.
    /// Logs a warning if a request takes longer than 500ms.
    /// </summary>
    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly Serilog.ILogger _logger;

        public PerformanceBehavior(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var start = DateTime.UtcNow;
            var response = await next();
            var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;

            if (elapsed > 500)
            {
                _logger.Warning("Slow request detected: {RequestName} took {Elapsed}ms. Request: {@Request}",
                    typeof(TRequest).Name, elapsed, request);
            }

            return response;
        }
    }
}

