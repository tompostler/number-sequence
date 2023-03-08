using Microsoft.AspNetCore.Http;

namespace number_sequence.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string GetClientIPAddress(this HttpRequest @this)
        {
            const string headerName = "X-Client-IP";
            return @this.Headers.ContainsKey(headerName)
                ? (string)@this.Headers[headerName]
                : @this.HttpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}
