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
            try
            {
                await this.next(httpContext);
            }
            finally
            {
                if (!httpContext.RequestAborted.IsCancellationRequested && !httpContext.Response.HasStarted)
                {
                    httpContext.Response.Headers.Append("X-Operation-Id", System.Diagnostics.Activity.Current?.Id);
                    httpContext.Response.Headers.Append("X-Operation-ParentId", System.Diagnostics.Activity.Current?.ParentId);
                    httpContext.Response.Headers.Append("X-Operation-RootId", System.Diagnostics.Activity.Current?.RootId);
                }
            }
        }
    }
}
