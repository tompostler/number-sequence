using Microsoft.Extensions.Logging;

namespace TcpWtf.NumberSequence.Tool
{
    internal static class EmptyTokenProvider
    {
        public static Task<string> GetAsync(ILogger logger, CancellationToken cancellationToken) => Task.FromResult(string.Empty);
    }
}
