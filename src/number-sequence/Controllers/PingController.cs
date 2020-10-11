using Microsoft.AspNetCore.Mvc;

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
    }
}
