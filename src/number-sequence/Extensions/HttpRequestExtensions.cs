using Microsoft.Extensions.Primitives;

namespace number_sequence.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string GetClientIPAddress(this HttpRequest @this)
        {
            // Cloudflare, https://developers.cloudflare.com/fundamentals/reference/http-headers/#cf-connecting-ip
            if (@this.Headers.TryGetValue("CF-Connecting-IP", out StringValues cloudflareIp))
            {
                return cloudflareIp;
            }
            // Azure App Service
            else if (@this.Headers.TryGetValue("X-Client-IP", out StringValues appServiceIp))
            {
                return appServiceIp;
            }
            else
            {
                return @this.HttpContext.Connection.RemoteIpAddress?.ToString();
            }
        }
    }
}
