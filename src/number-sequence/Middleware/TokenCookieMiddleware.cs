using number_sequence.Filters;
using number_sequence.Services;

namespace number_sequence.Middleware
{
    /// <summary>
    /// Passively reads the token cookie and populates HttpContext.User if valid.
    /// Does not reject requests! That is left to RequiresTokenFilter on protected endpoints.
    /// </summary>
    public sealed class TokenCookieMiddleware
    {
        private readonly RequestDelegate next;

        public TokenCookieMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, TokenValidationService validationService)
        {
            if (httpContext.Request.Cookies.TryGetValue(RequiresTokenFilter.TokenCookieName, out string token)
                && !string.IsNullOrWhiteSpace(token))
            {
                (RequiresTokenFilter.TokenPrincipal principal, string _) = await validationService.ValidateAsync(token, httpContext.RequestAborted);
                if (principal != null)
                {
                    httpContext.User = principal;
                }
            }

            await this.next(httpContext);
        }
    }
}
