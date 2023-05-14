using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
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
        public async Task<IActionResult> CreateAsync([FromBody] Token token, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Account account = await nsContext.Accounts.SingleOrDefaultAsync(x => x.Name == token.Account, cancellationToken);
            if (account == default)
            {
                return this.BadRequest($"Account with name [{token.Account}] does not exist.");
            }
            if (token.Key?.ComputeSHA256() != account.Key)
            {
                return this.Unauthorized($"Provided key did not match for account with name [{token.Account}].");
            }

            // Check if the account already exists
            if (await nsContext.Tokens.AnyAsync(x => x.Account == account.Name && x.Name == token.Name.ToLower(), cancellationToken))
            {
                return this.Conflict($"Token with name [{account.Name}] already exists.");
            }

            // Check if we shouldn't make another token for this account
            int tokenCountForAccount = await nsContext.Tokens.CountAsync(x => x.Account == account.Name, cancellationToken);
            if (tokenCountForAccount >= TierLimits.TokensPerAccount[account.Tier])
            {
                return this.Conflict($"Too many tokens already created for account with name [{account.Name}].");
            }

            Token toInsert = new()
            {
                Account = account.Name,
                AccountTier = account.Tier,
                ExpirationDate = token.ExpirationDate.ToUniversalTime(),
                Key = Guid.NewGuid().ToString(),
                Name = token.Name.ToLower(),
            };
            toInsert.Value = TokenValue.CreateFrom(toInsert).ToBase64JsonString();

            _ = nsContext.Tokens.Add(toInsert);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            Token createdToken = await nsContext.Tokens.SingleAsync(x => x.Account == account.Name && x.Name == toInsert.Name, cancellationToken);
            createdToken.Key = default;
            this.logger.LogInformation($"Created token: {createdToken.ToJsonString()}");
            return this.Ok(createdToken);
        }
    }
}
