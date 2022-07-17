using Microsoft.AspNetCore.Mvc;
using number_sequence.DataAccess;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class TokensController : ControllerBase
    {
        private readonly AccountDataAccess accountDataAccess;
        private readonly TokenDataAccess tokenDataAccess;

        public TokensController(AccountDataAccess accountDataAccess, TokenDataAccess tokenDataAccess)
        {
            this.accountDataAccess = accountDataAccess;
            this.tokenDataAccess = tokenDataAccess;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] Token token)
        {
            await this.accountDataAccess.ValidateAsync(token?.Account, token?.Key);
            Token createdToken = await this.tokenDataAccess.CreateAsync(token);
            return this.Ok(createdToken);
        }
    }
}
