using System.Net;
using System.Text.Json;
using PRN232.LMS.API.Models;

namespace PRN232.LMS.API.Middleware
{
    /// <summary>
    /// 🔴 YÊU CẦU 8: Global Exception Handling Middleware
    /// Xử lý tất cả unhandled exceptions và trả về consistent error response
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred while executing the request.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = new ErrorResponse
            {
                Success = false,
                Message = "Internal server error",
                Errors = null, // Không expose lỗi nội bộ - YÊU CẦU 8
                StatusCode = StatusCodes.Status500InternalServerError
            };

            // Log detailed error
            System.Diagnostics.Debug.WriteLine($"Exception: {exception.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {exception.StackTrace}");

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
