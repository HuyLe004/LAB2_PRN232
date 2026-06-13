using System.Diagnostics;

namespace PRN232.LMS.API.Middleware
{
    /// <summary>
    /// 🔴 YÊU CẦU 8: Request Logging Middleware
    /// Log: Request path, HTTP method, execution time, response status code
    /// </summary>
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
            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                var requestPath = context.Request.Path;
                var httpMethod = context.Request.Method;
                var statusCode = context.Response.StatusCode;
                var executionTime = stopwatch.ElapsedMilliseconds;

                var logMessage = $"[{httpMethod}] {requestPath} - Status: {statusCode} - Time: {executionTime}ms";

                if (statusCode >= 500)
                {
                    _logger.LogError(logMessage);
                }
                else if (statusCode >= 400)
                {
                    _logger.LogWarning(logMessage);
                }
                else
                {
                    _logger.LogInformation(logMessage);
                }
            }
        }
    }
}
