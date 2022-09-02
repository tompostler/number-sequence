using Microsoft.AspNetCore.Mvc;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Contracts;

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

        [HttpGet("debug")]
        public IActionResult GetDebug([FromQuery] string type, [FromQuery] bool content = false)
        {
            if (content)
            {
                return type switch
                {
                    "accepted" => this.Accepted(new { Value = "Ok" }),
                    _ => this.Ok(new { Value = "Ok" })
                };
            }
            else
            {
                return type switch
                {
                    "accepted" => this.Accepted(),
                    _ => this.Ok()
                };
            }
        }

        [HttpPost, RequiresToken]
        public IActionResult GetWithAuth()
        {
            return this.Ok();
        }

        [HttpPut, RequiresToken(AccountRoles.Ping)]
        public IActionResult GetWithAuthAndRole()
        {
            return this.Ok();
        }
    }
}
