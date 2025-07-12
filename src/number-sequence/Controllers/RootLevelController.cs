using Microsoft.AspNetCore.Mvc;

namespace number_sequence.Controllers
{
    [ApiController]
    public class RootLevelController : ControllerBase
    {
        private static ulong rootSequence = 0;

        [HttpGet("/")]
        public IActionResult Root() => this.Ok(++rootSequence);

        // https://github.com/MicrosoftDocs/azure-docs/blob/f71ba01af68bd4859cecce515e7eeeab2d8dd298/includes/app-service-web-configure-robots933456.md
        [HttpGet("/robots933456.txt")]
        public IActionResult AzureAppServiceRobotsTxt() => this.NoContent();

        [HttpGet("/favicon.ico")]
        public IActionResult Favicon() => this.File(Resources.Favicon, "image/x-icon");
    }
}
