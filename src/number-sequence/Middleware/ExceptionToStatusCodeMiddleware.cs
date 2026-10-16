using number_sequence.Exceptions;

namespace number_sequence.Middleware
{
    public sealed class ExceptionToStatusCodeMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionToStatusCodeMiddleware> logger;

        public ExceptionToStatusCodeMiddleware(RequestDelegate next, ILogger<ExceptionToStatusCodeMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await this.next(httpContext);
            }
            catch (Exception ex)
            {
                // The mapped exceptions are how an action reports an expected outcome (missing, forbidden, bad input) from
                // inside a helper. They get their message, not a stack trace, and aren't logged as errors.
                int? mappedStatusCode = ex switch
                {
                    BadRequestException => 400,
                    UnauthorizedException => 401,
                    ForbiddenException => 403,
                    NotFoundException => 404,
                    ConflictException => 409,
                    _ => null,
                };

                if (mappedStatusCode.HasValue)
                {
                    this.logger.LogInformation($"Returning {mappedStatusCode} for {ex.GetType().Name}: {ex.Message}");
                }
                else
                {
                    this.logger.LogError(ex, "Caught an unhandled exception!");
                }

                if (httpContext.Response.HasStarted)
                {
                    this.logger.LogError("Response in progress. Could not rewrite!");
                }
                else
                {
                    httpContext.Response.StatusCode = mappedStatusCode ?? 500;
                    httpContext.Response.ContentType = "text/plain";
                    await httpContext.Response.WriteAsync(mappedStatusCode.HasValue ? ex.Message : ex.ToString());
                }
            }
        }
    }
}
