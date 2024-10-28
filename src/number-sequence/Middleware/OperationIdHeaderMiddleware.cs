namespace number_sequence.Middleware
{
    public sealed class OperationIdHeaderMiddleware
    {
        private readonly RequestDelegate next;

        public OperationIdHeaderMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Response.Headers["Server-Operation-Id"] = System.Diagnostics.Activity.Current?.Id;
            httpContext.Response.Headers["Server-Operation-ParentId"] = System.Diagnostics.Activity.Current?.ParentId;
            httpContext.Response.Headers["Server-Operation-RootId"] = System.Diagnostics.Activity.Current?.RootId;
            await this.next(httpContext);
        }
    }
}
