using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;
using number_sequence.Models;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class TokensController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<TokensController> logger;

        public TokensController(
            IServiceProvider serviceProvider,
            ILogger<TokensController> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] Token requestedToken, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Account account = await nsContext.Accounts.SingleOrDefaultAsync(x => x.Name == requestedToken.Account, cancellationToken);
            if (account == default)
            {
                return this.BadRequest($"Account with name [{requestedToken.Account}] does not exist.");
            }
            if (requestedToken.Key?.ComputeSHA256() != account.Key)
            {
                return this.Unauthorized($"Provided key did not match for account with name [{requestedToken.Account}].");
            }

            // Check if the token already exists.
            // If it's expired, then we can recreate it in place.
            Token token = await nsContext.Tokens.SingleOrDefaultAsync(x => x.Account == this.User.Identity.Name && x.Name == requestedToken.Name.ToLower(), cancellationToken);
            if (token != default)
            {
                this.logger.LogInformation($"Found existing token: {token.ToJsonString()}");
                if (token.ExpirationDate > DateTimeOffset.UtcNow)
                {
                    return this.Conflict($"Token with name [{requestedToken.Name}] already exists and is not expired.");
                }
            }

            // Check if we shouldn't make another token for this account.
            int tokenCountForAccount = await nsContext.Tokens.CountAsync(x => x.Account == account.Name, cancellationToken);
            if (tokenCountForAccount >= TierLimits.TokensPerAccount[account.Tier])
            {
                return this.Conflict($"Too many tokens already created for account with name [{account.Name}].");
            }

            // Init if not found.
            if (token == default)
            {
                token = new()
                {
                    Account = account.Name,
                    AccountTier = account.Tier,
                    Name = requestedToken.Name.ToLower(),
                };
                _ = nsContext.Tokens.Add(token);
            }

            // Finish init/update.
            token.Key = Guid.NewGuid().ToString(); // Junk for DB storage due to [Required] attribute.
            token.ExpirationDate = requestedToken.ExpirationDate.ToUniversalTime();
            token.Value = TokenValue.CreateFrom(token).ToBase64JsonString();

            _ = await nsContext.SaveChangesAsync(cancellationToken);

            Token createdToken = await nsContext.Tokens.SingleAsync(x => x.Account == account.Name && x.Name == token.Name, cancellationToken);
            createdToken.Key = default;
            this.logger.LogInformation($"Created token: {createdToken.ToJsonString()}");
            return this.Ok(createdToken);
        }

        [RequiresToken]
        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteAsync(string name, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Token token = await nsContext.Tokens.SingleOrDefaultAsync(x => x.Account == this.User.Identity.Name && x.Name == name, cancellationToken);
            if (token == default)
            {
                return this.NotFound($"Token with name [{name}] does not exist for your account.");
            }

            _ = nsContext.Tokens.Remove(token);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            token.Key = default;
            this.logger.LogInformation($"Deleted token: {token.ToJsonString()}");
            return this.Ok(token);
        }
    }
}
