using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using number_sequence.DataAccess;
using number_sequence.Extensions;
using number_sequence.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Filters
{
    public sealed class RequiresTokenAttribute : ActionFilterAttribute
    {
        private readonly TokenDataAccess tokenDataAccess;
        private readonly IMemoryCache memoryCache;
        private readonly ILogger<RequiresTokenAttribute> logger;

        public RequiresTokenAttribute(TokenDataAccess tokenDataAccess, IMemoryCache memoryCache, ILogger<RequiresTokenAttribute> logger)
        {
            this.tokenDataAccess = tokenDataAccess;
            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // If a token url parameter is provided, that supersedes the check for the Authorization header
            string token = default;
            if (context.HttpContext.Request.Query.TryGetValue("token", out var tokenValues))
            {
                token = tokenValues.First();
                this.logger.LogInformation("Token found in URL.");
            }


            // So try to read it from the auth header
            if (string.IsNullOrWhiteSpace(token))
            {
                if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeaders))
                {
                    token = authHeaders.FirstOrDefault(ah => ah.StartsWith("Token "))?.Split(' ').Last();
                    this.logger.LogInformation("Token found in headers.");
                }
            }

            // No valid token
            if (string.IsNullOrWhiteSpace(token))
            {
                this.logger.LogWarning("No token found.");
                context.Result = new UnauthorizedObjectResult("No token found.");
            }

            // See if it's in the cache. That would mean it's valid
            if (this.memoryCache.TryGetValue(token, out TokenPrincipal principal))
            {
                this.logger.LogInformation("Token found in cache.");
                context.HttpContext.User = principal;
            }
            else
            {
                // Try to parse it
                TokenValue tokenValue = default;
                try
                {
                    tokenValue = token.FromBase64JsonString<TokenValue>();

                    // Ensure it is useful before bothering to see if it is valid
                    var validationContext = new ValidationContext(tokenValue);
                    var validationResults = new List<ValidationResult>();
                    bool isValid = Validator.TryValidateObject(tokenValue, validationContext, validationResults, validateAllProperties: true);
                    if (!isValid)
                    {
                        var validationResultString = $"Token model not valid:\n{string.Join('\n', validationResults)}";
                        this.logger.LogWarning(validationResultString);
                        context.Result = new UnauthorizedObjectResult(validationResultString);
                    }
                    else
                    {
                        // See if a token by that name exists
                        var tokenModel = await this.tokenDataAccess.TryGetAsync(tokenValue.Account, tokenValue.Name);
                        if (tokenModel == default)
                        {
                            this.logger.LogWarning("Token not found.");
                            context.Result = new UnauthorizedObjectResult("Token not found.");
                        }
                        else
                        {
                            // Ensure it matches
                            if (tokenValue.Account != tokenModel.Account
                                || tokenValue.AccountTier != tokenModel.AccountTier
                                || tokenValue.CreatedAt != tokenModel.CreatedAt
                                || tokenValue.ExpiresAt != tokenModel.ExpiresAt
                                || tokenValue.Key != tokenModel.Key
                                || tokenValue.Name != tokenModel.Name)
                            {
                                this.logger.LogWarning("Token not valid.");
                                context.Result = new UnauthorizedObjectResult("Token not valid.");
                            }
                            else
                            {
                                this.logger.LogInformation($"Token is valid: {tokenValue.Account}/{tokenValue.Name}");
                                principal = new TokenPrincipal(new GenericIdentity(tokenValue.Account)) { Token = tokenValue };
                                context.HttpContext.User = principal;

                                this.memoryCache.Set(
                                    token,
                                    principal,
                                    new MemoryCacheEntryOptions
                                    {
                                        AbsoluteExpiration = tokenValue.ExpiresAt,
                                        Priority = (CacheItemPriority)TierLimits.CacheItemPriority[tokenValue.AccountTier],
                                        Size = 1,
                                        SlidingExpiration = TimeSpan.FromMinutes(5)
                                    });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning(ex, "Could not parse token.");
                    context.Result = new UnauthorizedObjectResult("Token malformed.");
                }
            }

            await next();
        }

        public sealed class TokenPrincipal : GenericPrincipal
        {
            public TokenPrincipal(IIdentity identity)
                : base(identity, default)
            {
            }

            public TokenValue Token { get; set; }
        }
    }
}
