using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using number_sequence.Models;
using number_sequence.Services;
using System.Security.Principal;

namespace number_sequence.Filters
{
    public sealed class RequiresTokenFilter : IAsyncAuthorizationFilter
    {
        public const string TokenCookieName = "ns-token";

        private readonly string requiredRole;
        private readonly ILogger<RequiresTokenFilter> logger;

        public RequiresTokenFilter(string requiredRole, ILogger<RequiresTokenFilter> logger)
        {
            this.requiredRole = requiredRole;
            this.logger = logger;
        }

        private static IActionResult Unauthorized(AuthorizationFilterContext context, string message)
        {
            if (context.HttpContext.Request.Path.StartsWithSegments("/ui"))
            {
                string returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                return new RedirectResult($"/ui/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }
            return new UnauthorizedObjectResult(message);
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Query param supersedes all others
            string token = default;
            if (context.HttpContext.Request.Query.TryGetValue("token", out Microsoft.Extensions.Primitives.StringValues tokenValues))
            {
                token = tokenValues.First();
                this.logger.LogInformation("Token found in URL.");
            }

            // Then Authorization header
            if (string.IsNullOrWhiteSpace(token))
            {
                if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues authHeaders))
                {
                    token = authHeaders.FirstOrDefault(ah => ah.StartsWith("Token "))?.Split(' ').Last();
                    this.logger.LogInformation("Token found in headers.");
                }
            }

            // Then cookie
            if (string.IsNullOrWhiteSpace(token))
            {
                if (context.HttpContext.Request.Cookies.TryGetValue(TokenCookieName, out string cookieToken))
                {
                    token = cookieToken;
                    this.logger.LogInformation("Token found in cookie.");
                }
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                this.logger.LogWarning("No token found.");
                context.Result = Unauthorized(context, "No token found.");
                return;
            }

            TokenValidationService validationService = context.HttpContext.RequestServices.GetRequiredService<TokenValidationService>();
            (TokenPrincipal principal, string error) = await validationService.ValidateAsync(token, context.HttpContext.RequestAborted);

            if (principal == null)
            {
                context.Result = Unauthorized(context, error);
                return;
            }

            if (!string.IsNullOrEmpty(this.requiredRole) && !principal.IsInRole(this.requiredRole))
            {
                this.logger.LogWarning($"Account does not have the {this.requiredRole} role.");
                context.Result = Unauthorized(context, $"Account is missing the {this.requiredRole} role.");
                return;
            }

            context.HttpContext.User = principal;
        }

        public sealed class TokenPrincipal : GenericPrincipal
        {
            public TokenPrincipal(IIdentity identity, string[] roles)
                : base(identity, roles)
            { }

            public string RawToken { get; set; }
        }
    }
}
