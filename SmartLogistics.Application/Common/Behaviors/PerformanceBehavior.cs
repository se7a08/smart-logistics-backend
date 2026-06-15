using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SmartLogistics.Application.Common.Behaviors
{
    
    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

        public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            
            var timer = new Stopwatch();
            timer.Start();

            var response = await next();

            timer.Stop();

            var elapsedMilliseconds = timer.ElapsedMilliseconds;

            
            if (elapsedMilliseconds > 500)
            {
                var requestName = typeof(TRequest).Name;

                _logger.LogWarning($"Slow Request: {requestName} took {elapsedMilliseconds}ms");
            }

            return response;
        }
    }
}