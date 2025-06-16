using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Caching.Memory;
using number_sequence.Extensions;

namespace number_sequence.Controllers
{
    [ApiController, Route("[controller]")]
    public sealed class DebugController : ControllerBase
    {
        private readonly IMemoryCache memoryCache;
        private readonly IEnumerable<EndpointDataSource> endpointSources;

        public DebugController(
            IMemoryCache memoryCache,
            IEnumerable<EndpointDataSource> endpointSources)
        {
            this.memoryCache = memoryCache;
            this.endpointSources = endpointSources;
        }

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

        //[HttpGet("dumproutes")]
        //public IActionResult DumpCache()
        //{
        //    IEnumerable<RouteEndpoint> endpoints = this.endpointSources.SelectMany(es => es.Endpoints).OfType<RouteEndpoint>();
        //    var output = endpoints.Select(
        //        e =>
        //        {
        //            ControllerActionDescriptor controller = e.Metadata
        //                .OfType<ControllerActionDescriptor>()
        //                .FirstOrDefault();
        //            string action = controller != null
        //                ? $"{controller.ControllerName}.{controller.ActionName}"
        //                : null;
        //            string controllerMethod = controller != null
        //                ? $"{controller.ControllerTypeInfo.FullName}:{controller.MethodInfo.Name}"
        //                : null;
        //            return new
        //            {
        //                Method = e.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods?[0],
        //                Route = $"/{e.RoutePattern.RawText.TrimStart('/')}",
        //                Action = action,
        //                ControllerMethod = controllerMethod
        //            };
        //        }
        //    );

        //    return this.Ok(output);
        //}

        [HttpGet("request")]
        public IActionResult HttpGetRequest()
            => this.Ok(
                new
                {
                    ManualRemoteIpAddress = this.Request.GetClientIPAddress(),
                    RemoteIpAddress = this.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    this.HttpContext.Request.Headers,
                });

        //[HttpGet("slowrequest")]
        //public async Task<IActionResult> SlowRequest([FromQuery] int seconds = 5)
        //{
        //    await Task.Delay(TimeSpan.FromSeconds(seconds));
        //    return this.Ok($"Waited {seconds} seconds.");
        //}
    }
}
