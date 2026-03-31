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
            httpContext.Response.OnStarting(() =>
            {
                httpContext.Response.Headers[HttpHeaderNames.ServerOperationId] = System.Diagnostics.Activity.Current?.RootId;
#pragma warning disable CS0436 // Type conflicts with imported type
                httpContext.Response.Headers[HttpHeaderNames.ServerVersion] = ThisAssembly.AssemblyInformationalVersion;
#pragma warning restore CS0436 // Type conflicts with imported type
                return Task.CompletedTask;
            });
            await this.next(httpContext);
        }
    }
}
