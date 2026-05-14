using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SmartLogistics.API.Middleware
{
    // ميدل وير لتسجيل كل طلب (Request) بيدخل السيستم والرد (Response) اللي بيخرج منه
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // بنبدأ عداد الوقت عشان نعرف الـ Request أخد وقت قد إيه
            var timer = Stopwatch.StartNew();
            var traceId = context.TraceIdentifier;

            // تسجيل الدخول (Incoming Request)
            _logger.LogInformation("Incoming Request: {Method} {Path} [TraceId: {TraceId}]",
                context.Request.Method,
                context.Request.Path,
                traceId);

            // بنمرر الطلب للي بعده في الـ Pipeline
            await _next(context);

            timer.Stop();
            var elapsedMs = timer.ElapsedMilliseconds;
            var statusCode = context.Response.StatusCode;

            // بنحدد درجة الأهمية (Level) بناءً على حالة الرد
            if (statusCode >= 500)
            {
                _logger.LogError("Request Error: {Method} {Path} responded {StatusCode} in {Elapsed}ms",
                    context.Request.Method, context.Request.Path, statusCode, elapsedMs);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning("Request Warning: {Method} {Path} responded {StatusCode} in {Elapsed}ms",
                    context.Request.Method, context.Request.Path, statusCode, elapsedMs);
            }
            else
            {
                _logger.LogInformation("Request Success: {Method} {Path} responded {StatusCode} in {Elapsed}ms",
                    context.Request.Method, context.Request.Path, statusCode, elapsedMs);
            }
        }
    }
}