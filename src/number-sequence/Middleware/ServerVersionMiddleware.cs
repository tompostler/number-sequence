using TcpWtf.NumberSequence.Contracts.Framework;

namespace number_sequence.Middleware
{
    public sealed class ServerVersionMiddleware
    {
        private readonly RequestDelegate next;

        public ServerVersionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            string assemblyFileVersion = ThisAssembly.AssemblyInformationalVersion;
            httpContext.Response.Headers[HttpHeaderNames.ServerVersion] = assemblyFileVersion;
            await this.next(httpContext);
        }
    }
}
