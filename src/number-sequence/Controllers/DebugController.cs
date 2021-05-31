using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace number_sequence.Controllers
{
    [ApiController, Route("[controller]")]
    public sealed class DebugController : ControllerBase
    {
        //private readonly IMemoryCache memoryCache;

        //public DebugController(IMemoryCache memoryCache)
        //{
        //    this.memoryCache = memoryCache;
        //}

        //[HttpGet("dumpcache")]
        //public IActionResult DumpCache()
        //{
        //    FieldInfo field = typeof(MemoryCache).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);
        //    var obj = (IDictionary)field.GetValue(this.memoryCache);

        //    var values = new List<object>();
        //    foreach (var value in obj.Values)
        //    {
        //        var type = value.GetType();
        //        values.Add(new
        //        {
        //            AbsoluteExpiration = type.GetProperty("AbsoluteExpiration")?.GetValue(value),
        //            AbsoluteExpirationRelativeToNow = type.GetProperty("AbsoluteExpirationRelativeToNow")?.GetValue(value),
        //            SlidingExpirationTotalMinutes = ((TimeSpan?)type.GetProperty("SlidingExpiration")?.GetValue(value))?.TotalMinutes,
        //            Priority = type.GetProperty("Priority")?.GetValue(value),
        //            Size = type.GetProperty("Size")?.GetValue(value),
        //            Key = type.GetProperty("Key")?.GetValue(value),
        //            Value = type.GetProperty("Value")?.GetValue(value)?.ToString(),
        //            LastAccessed = type.GetProperty("LastAccessed")?.GetValue(value),
        //            EvictionReason = type.GetProperty("EvictionReason")?.GetValue(value)
        //        });
        //    }

        //    return this.Ok(values);
        //}

        [HttpGet("slowrequest")]
        public async Task<IActionResult> SlowRequest([FromQuery] int seconds = 5)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            return this.Ok($"Waited {seconds} seconds.");
        }

        [HttpGet("robots933456.txt")]
        public IActionResult AzureAppServiceRobotsTxt()
        {
            // https://github.com/MicrosoftDocs/azure-docs/blob/f71ba01af68bd4859cecce515e7eeeab2d8dd298/includes/app-service-web-configure-robots933456.md
            return this.NoContent();
        }
    }
}
