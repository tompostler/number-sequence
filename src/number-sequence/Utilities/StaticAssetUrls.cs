using Microsoft.AspNetCore.Components;

namespace number_sequence.Utilities
{
    /// <summary>
    /// The fingerprinted url of a file under <c>wwwroot</c>, for one that script fetches by itself rather than through a
    /// tag that <c>asp-append-version</c> handles, such as the scanner's <c>.wasm</c>. It resolves through the same
    /// manifest the tag helpers use, which <c>WithStaticAssets()</c> attaches to every page's endpoint, so the url is the
    /// immutable one. A path that isn't in the manifest comes back as the plain route.
    /// </summary>
    public static class StaticAssetUrls
    {
        /// <param name="path">Relative to <c>wwwroot</c>, with no leading slash, such as <c>lib/zxing-wasm/zxing_reader.wasm</c>.</param>
        public static string For(HttpContext httpContext, string path)
        {
            ResourceAssetCollection assets = httpContext.GetEndpoint()?.Metadata.GetMetadata<ResourceAssetCollection>();
            return $"{httpContext.Request.PathBase}/{assets?[path] ?? path}";
        }
    }
}
