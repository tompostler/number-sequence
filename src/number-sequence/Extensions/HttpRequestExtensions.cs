using Microsoft.AspNetCore.Http;
using System.Net;

namespace number_sequence.Extensions
{
    public static class HttpRequestExtensions
    {
        public static IPAddress GetIPAddress(this HttpRequest @this)
        {
            if (@this.Headers.ContainsKey("X-Forwarded-For") && IPAddress.TryParse(@this.Headers["X-Forwarded-For"], out IPAddress address))
            {
                return address;
            }
            return default;
        }
    }
}
