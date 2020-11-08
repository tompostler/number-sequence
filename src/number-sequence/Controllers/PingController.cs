using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;

namespace number_sequence.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class PingController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return this.Ok();
        }

        [HttpPost, RequiresToken]
        public IActionResult GetWithAuth()
        {
            return this.Ok();
        }
    }
}
