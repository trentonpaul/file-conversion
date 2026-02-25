using file_conversion_api.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace file_conversion_api.Services
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            ExceptionDetails problemDetails;

            if (_env.IsDevelopment())
            {
                problemDetails = new ExceptionDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred.",
                    Detail = exception.ToString(),
                    Instance = httpContext.Request.Path
                };
            }
            else
            {
                problemDetails = new ExceptionDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred.",
                    Instance = httpContext.Request.Path
                };
            }

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true; // true means the exception is handled and the pipeline is short-circuited
        }
    }
}