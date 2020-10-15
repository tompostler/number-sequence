using Microsoft.AspNetCore.Mvc;
using number_sequence.DataAccess;
using System.Threading.Tasks;

namespace number_sequence.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class AccountsController : ControllerBase
    {
        private readonly AccountDataAccess accountDataAccess;

        public AccountsController(AccountDataAccess accountDataAccess)
        {
            this.accountDataAccess = accountDataAccess;
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetAsync(string name)
        {
            var account = await this.accountDataAccess.GetAsync(name);
            if (account == default)
                return this.NotFound();
            else
                return this.Ok(account);
        }
    }
}
