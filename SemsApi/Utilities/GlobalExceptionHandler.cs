using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SemsApi.Exceptions;
using System.Net;

namespace SemsApi.Utilities
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, message) = GetExceptionDetails(exception);
            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = message }, cancellationToken);
            return true;
        }

        private static (int status, string message) GetExceptionDetails(Exception ex) => ex switch
        {
            RefreshTokenException => (StatusCodes.Status401Unauthorized, ex.Message),
            ExternalLoginProviderException => (StatusCodes.Status400BadRequest, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };
    }
}
