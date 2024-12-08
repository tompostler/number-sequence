using Microsoft.AspNetCore.Mvc;

namespace number_sequence.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class HistoryController : ControllerBase
    {
        [HttpGet]
        public IActionResult History()
            => this.Ok(TcpWtf.NumberSequence.Contracts.History.Commits);
    }
}
