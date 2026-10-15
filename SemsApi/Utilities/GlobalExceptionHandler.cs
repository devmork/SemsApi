using Microsoft.AspNetCore.Diagnostics;
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
            var (statusCode, message) = GetExceptionDetails(exception);

            _logger.LogError(exception, exception.Message);

            httpContext.Response.StatusCode = (int)statusCode;
            await httpContext.Response.WriteAsJsonAsync(message, cancellationToken);

            return true;
        }

        private (HttpStatusCode statusCode, string message) GetExceptionDetails(Exception exception)
        {
            return exception switch
            {
                RefreshTokenException => (HttpStatusCode.Unauthorized, exception.Message),
                ExternalLoginProviderException => (HttpStatusCode.BadRequest, exception.Message),
                _ => (HttpStatusCode.InternalServerError, $"An unexpected error occurred: {exception.Message}")
            };
        }
    }
}
