using System.Threading;
using System.Threading.Tasks;

namespace TcpWtf.NumberSequence.Tool
{
    internal static class EmptyTokenProvider
    {
        public static Task<string> GetTokenAsync(CancellationToken _) => Task.FromResult(string.Empty);
    }
}
