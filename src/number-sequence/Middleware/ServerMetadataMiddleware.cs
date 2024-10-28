using TcpWtf.NumberSequence.Contracts.Framework;

namespace number_sequence.Middleware
{
    public sealed class ServerMetadataMiddleware
    {
        private readonly RequestDelegate next;

        public ServerMetadataMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Response.Headers[HttpHeaderNames.ServerOperationId] = System.Diagnostics.Activity.Current?.RootId;

            string assemblyFileVersion = ThisAssembly.AssemblyInformationalVersion;
            httpContext.Response.Headers[HttpHeaderNames.ServerVersion] = assemblyFileVersion;

            await this.next(httpContext);
        }
    }
}
