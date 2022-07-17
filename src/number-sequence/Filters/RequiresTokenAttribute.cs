using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
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
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ILogger<RequiresTokenAttribute> logger = context.HttpContext.RequestServices.GetService<ILogger<RequiresTokenAttribute>>();

            // If a token url parameter is provided, that supersedes the check for the Authorization header
            string token = default;
            if (context.HttpContext.Request.Query.TryGetValue("token", out Microsoft.Extensions.Primitives.StringValues tokenValues))
            {
                token = tokenValues.First();
                logger.LogInformation("Token found in URL.");
            }


            // So try to read it from the auth header
            if (string.IsNullOrWhiteSpace(token))
            {
                if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues authHeaders))
                {
                    token = authHeaders.FirstOrDefault(ah => ah.StartsWith("Token "))?.Split(' ').Last();
                    logger.LogInformation("Token found in headers.");
                }
            }

            // No valid token
            if (string.IsNullOrWhiteSpace(token))
            {
                logger.LogWarning("No token found.");
                context.Result = new UnauthorizedObjectResult("No token found.");
                return;
            }

            // See if it's in the cache. That would mean it's valid
            logger.LogInformation($"Token: {token}");
            IMemoryCache memoryCache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
            if (memoryCache.TryGetValue(token, out TokenPrincipal principal))
            {
                logger.LogInformation("Token found in cache.");
                context.HttpContext.User = principal;
            }
            else
            {
                // Try to parse it
                TokenValue tokenValue = default;
                try
                {
                    tokenValue = token.FromBase64JsonString<TokenValue>();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Could not parse token.");
                    context.Result = new UnauthorizedObjectResult("Token malformed.");
                    return;
                }

                // Ensure it is useful before bothering to see if it is valid
                var validationContext = new ValidationContext(tokenValue);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(tokenValue, validationContext, validationResults, validateAllProperties: true);
                if (!isValid)
                {
                    string validationResultString = $"Token model not valid:\n{string.Join('\n', validationResults)}";
                    logger.LogWarning(validationResultString);
                    context.Result = new UnauthorizedObjectResult(validationResultString);
                    return;
                }
                else
                {
                    // See if a token by that name exists
                    TokenDataAccess tokenDataAccess = context.HttpContext.RequestServices.GetService<TokenDataAccess>();
                    Token tokenModel = await tokenDataAccess.TryGetAsync(tokenValue.Account, tokenValue.Name);
                    if (tokenModel == default)
                    {
                        logger.LogWarning("Token not found.");
                        context.Result = new UnauthorizedObjectResult("Token not found.");
                        return;
                    }
                    else
                    {
                        // Ensure it matches
                        if (token != tokenModel.Value)
                        {
                            logger.LogWarning("Token not valid.");
                            context.Result = new UnauthorizedObjectResult("Token not valid.");
                            return;
                        }
                        else
                        {
                            logger.LogInformation($"Token is valid: {tokenValue.Account}/{tokenValue.Name}");
                            principal = new TokenPrincipal(new GenericIdentity(tokenValue.Account)) { Token = tokenValue };
                            context.HttpContext.User = principal;

                            _ = memoryCache.Set(
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

            _ = await next();
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
