using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using number_sequence.DataAccess;
using System;
using System.Threading;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class TokensController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly TokenDataAccess tokenDataAccess;

        public TokensController(
            IServiceProvider serviceProvider,
            TokenDataAccess tokenDataAccess)
        {
            this.serviceProvider = serviceProvider;
            this.tokenDataAccess = tokenDataAccess;
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

            Token createdToken = await this.tokenDataAccess.CreateAsync(token);
            return this.Ok(createdToken);
        }
    }
}
