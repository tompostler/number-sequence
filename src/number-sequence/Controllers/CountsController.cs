using Microsoft.AspNetCore.Mvc;
using number_sequence.DataAccess;
using number_sequence.Filters;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Controllers
{
    [ApiController, Route("[controller]"), RequiresToken]
    public sealed class CountsController : ControllerBase
    {
        private readonly CountDataAccess countDataAccess;

        public CountsController(CountDataAccess countDataAccess)
        {
            this.countDataAccess = countDataAccess;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] Count count)
        {
            count.Account = this.User.Identity.Name;
            var createdCount = await this.countDataAccess.CreateAsync(count);
            return this.Ok(createdCount);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetAsync(string name)
        {
            var count = await this.countDataAccess.TryGetAsync(this.User.Identity.Name, name);
            return count == default
                ? (IActionResult)this.NotFound()
                : this.Request.Query.ContainsKey("bare")
                    ? this.Ok(count.Value)
                    : this.Ok(count);
        }

        [HttpPut("{name}")]
        public async Task<IActionResult> IncrementAsync(string name)
        {
            var count = await this.countDataAccess.IncrementAsync(this.User.Identity.Name, name);
            return this.Request.Query.ContainsKey("bare")
                ? this.Ok(count.Value)
                : this.Ok(count);
        }

        [HttpPut("{name}/{amount}")]
        public async Task<IActionResult> IncrementByAmountAsync(string name, ulong amount)
        {
            var count = await this.countDataAccess.IncrementByAmountAsync(this.User.Identity.Name, name, amount);
            return this.Request.Query.ContainsKey("bare")
                ? this.Ok(count.Value)
                : this.Ok(count);
        }
    }
}
