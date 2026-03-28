using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using number_sequence.DataAccess;
using number_sequence.Filters;
using number_sequence.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.Services
{
    public sealed class TokenValidationService
    {
        private readonly NsContext nsContext;
        private readonly IMemoryCache memoryCache;
        private readonly ILogger<TokenValidationService> logger;

        public TokenValidationService(NsContext nsContext, IMemoryCache memoryCache, ILogger<TokenValidationService> logger)
        {
            this.nsContext = nsContext;
            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        /// <summary>
        /// Validates a token string. Returns the principal on success, or null with an error message on failure.
        /// </summary>
        public async Task<(RequiresTokenFilter.TokenPrincipal Principal, string Error)> ValidateAsync(string token, CancellationToken cancellationToken)
        {
            // Cache hit means already validated
            if (this.memoryCache.TryGetValue(token, out RequiresTokenFilter.TokenPrincipal principal))
            {
                this.logger.LogInformation("Token found in cache.");
                return (principal, null);
            }

            // Parse
            TokenValue tokenValue;
            try
            {
                tokenValue = token.FromBase64JsonString<TokenValue>();
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Could not parse token.");
                return (null, "Token malformed.");
            }

            // Model validation (includes expiration via CustomValidation attribute)
            var validationContext = new ValidationContext(tokenValue);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(tokenValue, validationContext, validationResults, validateAllProperties: true))
            {
                string message = $"Token model not valid:\n{string.Join('\n', validationResults)}";
                this.logger.LogWarning(message);
                return (null, message);
            }

            // DB lookup
            Token tokenModel = await this.nsContext.Tokens.SingleOrDefaultAsync(x => x.Account == tokenValue.Account && x.Name == tokenValue.Name, cancellationToken);
            if (tokenModel == default)
            {
                this.logger.LogWarning("Token not found.");
                return (null, "Token not found.");
            }
            if (token != tokenModel.Value)
            {
                this.logger.LogWarning("Token not valid.");
                return (null, "Token not valid.");
            }

            // Build principal
            this.logger.LogInformation($"Token is valid: {tokenValue.Account}/{tokenValue.Name}");
            Account account = await this.nsContext.Accounts.SingleAsync(x => x.Name == tokenValue.Account, cancellationToken);
            principal = new RequiresTokenFilter.TokenPrincipal(new GenericIdentity(tokenValue.Account), (account.Roles ?? string.Empty).Split(';')) { Token = tokenValue };

            _ = this.memoryCache.Set(
                token,
                principal,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = tokenValue.ExpirationDate,
                    Priority = (CacheItemPriority)TierLimits.CacheItemPriority[tokenValue.AccountTier],
                    Size = 1,
                    SlidingExpiration = TimeSpan.FromMinutes(5),
                });

            return (principal, null);
        }
    }
}
