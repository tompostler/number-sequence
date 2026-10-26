using SkiaSharp;
using System.Diagnostics;
using System.Net;

namespace number_sequence.Services.Libraries
{
    /// <summary>
    /// Fetches and prepares item covers. Every cover, uploaded or fetched, is decoded and re-encoded here as a WebP of a
    /// fixed size plus a thumbnail. That is what makes storing and serving them safe: whatever was sent, what's stored is an
    /// image this code drew, with no metadata and no way to be an svg or html page served from this site.
    /// <para>
    /// Fetching is only from lookup provider hosts (<see cref="AllowedHostSuffixes"/>), checked again on every redirect,
    /// because the urls come from the request. Without that, the cover endpoint would fetch any url for anyone with the
    /// Library role, including addresses inside the app service.
    /// </para>
    /// </summary>
    public sealed class LibraryCovers
    {
        public const string HttpClientName = "library-cover";

        /// <summary>
        /// What every stored and previewed cover is. WebP is a quarter to a third smaller than JPEG at the same quality,
        /// which matters on a list of thumbnails over a phone connection, and every current browser shows it. PNG would be
        /// several times larger: covers are photos and artwork, which lossless compression handles badly.
        /// </summary>
        public const string ContentType = "image/webp";

        /// <summary>
        /// On the blob names, so a cover downloaded by hand from the storage account opens. Changing the format changes the
        /// blob paths with it, so existing covers would need renaming or re-encoding at the same time.
        /// </summary>
        public const string FileExtension = ".webp";

        /// <summary>
        /// High rather than the usual 75–80, so that stored covers stay good enough to re-encode later (a new size, say)
        /// without starting from a visibly degraded copy. Covers are small enough that the extra bytes don't matter.
        /// </summary>
        private const int Quality = 90;

        /// <summary>
        /// The longest edge of a stored cover. A typical 2:3 cover is then 600 wide, which is plenty for a phone screen.
        /// </summary>
        public const int CoverMaxEdge = 900;

        /// <summary>
        /// The longest edge of a thumbnail, for lists of search results. Served at 48 or so css pixels wide, so this covers
        /// a 3x screen.
        /// </summary>
        public const int ThumbnailMaxEdge = 240;

        /// <summary>
        /// The largest image accepted, uploaded or fetched. A phone photo is 3–8 MB; BoardGameGeek's original images can be
        /// several MB too.
        /// </summary>
        public const int MaxInputBytes = 15 * 1024 * 1024;

        private const long MaxInputPixels = 50_000_000;

        private const int MaxRedirects = 3;

        /// <summary>
        /// Hosts lookup results point covers at, and the hosts those redirect to. Open Library's cover urls redirect to
        /// archive.org storage.
        /// </summary>
        private static readonly string[] AllowedHostSuffixes =
        [
            "openlibrary.org",
            "archive.org",
            "books.google.com",
            "googleusercontent.com",
            "tmdb.org",
            "geekdo-images.com",
        ];

        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<LibraryCovers> logger;

        public LibraryCovers(IHttpClientFactory httpClientFactory, ILogger<LibraryCovers> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        public sealed record Prepared(byte[] Cover, byte[] Thumbnail);

        public static bool IsAllowed(string url)
            => Uri.TryCreate(url, UriKind.Absolute, out Uri uri) && IsAllowed(uri);

        private static bool IsAllowed(Uri uri)
            => uri.Scheme == Uri.UriSchemeHttps
                && uri.IsDefaultPort
                && AllowedHostSuffixes.Any(x => uri.Host.Equals(x, StringComparison.OrdinalIgnoreCase) || uri.Host.EndsWith("." + x, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// The first of <paramref name="urls"/> that fetches and decodes as an image, prepared. Null when none do; each
        /// failure is logged.
        /// </summary>
        public async Task<Prepared> FetchAndPrepareAsync(IEnumerable<string> urls, CancellationToken cancellationToken)
        {
            foreach (string url in urls.Where(x => !string.IsNullOrWhiteSpace(x)).Take(4))
            {
                byte[] data = await this.FetchAsync(url, cancellationToken);
                Prepared prepared = data == null ? null : this.Prepare(data);
                if (prepared != null)
                {
                    return prepared;
                }
            }
            return null;
        }

        /// <summary>
        /// One image from an allowed host, unchanged, or null if it can't be fetched. Used to preview a lookup result's
        /// cover through this site rather than hotlinking it.
        /// </summary>
        public async Task<byte[]> FetchAsync(string url, CancellationToken cancellationToken)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                return null;
            }

            Stopwatch sw = Stopwatch.StartNew();
            HttpClient client = this.httpClientFactory.CreateClient(HttpClientName);
            for (int hop = 0; hop <= MaxRedirects; hop++)
            {
                if (!IsAllowed(uri))
                {
                    this.logger.LogInformation($"Cover fetch of [{url}] refused: [{uri}] is not a lookup provider host.");
                    return null;
                }

                try
                {
                    using HttpResponseMessage response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    if (response.StatusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Found or HttpStatusCode.SeeOther or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect
                        && response.Headers.Location != null)
                    {
                        uri = new Uri(uri, response.Headers.Location);
                        continue;
                    }
                    if (!response.IsSuccessStatusCode)
                    {
                        this.logger.LogInformation($"Cover fetch of [{uri}] returned {(int)response.StatusCode} after {sw.ElapsedMilliseconds}ms.");
                        return null;
                    }
                    if (response.Content.Headers.ContentLength > MaxInputBytes)
                    {
                        this.logger.LogInformation($"Cover fetch of [{uri}] skipped: {response.Content.Headers.ContentLength:N0} bytes is over the limit.");
                        return null;
                    }

                    using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    using MemoryStream buffer = new();
                    byte[] chunk = new byte[80 * 1024];
                    int read;
                    while ((read = await stream.ReadAsync(chunk, cancellationToken)) > 0)
                    {
                        buffer.Write(chunk, 0, read);
                        if (buffer.Length > MaxInputBytes)
                        {
                            this.logger.LogInformation($"Cover fetch of [{uri}] abandoned: over {MaxInputBytes:N0} bytes.");
                            return null;
                        }
                    }
                    this.logger.LogInformation($"Cover fetch of [{uri}] got {buffer.Length:N0} bytes in {sw.ElapsedMilliseconds}ms.");
                    return buffer.ToArray();
                }
                catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
                {
                    this.logger.LogInformation($"Cover fetch of [{uri}] failed after {sw.ElapsedMilliseconds}ms: {ex.Message}");
                    return null;
                }
            }

            this.logger.LogInformation($"Cover fetch of [{url}] gave up after {MaxRedirects} redirects.");
            return null;
        }

        /// <summary>
        /// The cover and thumbnail for an image in any format Skia decodes, turned upright per its EXIF orientation (phone
        /// photos are stored sideways and flagged), or null if it isn't an image.
        /// </summary>
        public Prepared Prepare(byte[] data)
        {
            Stopwatch sw = Stopwatch.StartNew();
            using SKCodec codec = SKCodec.Create(new SKMemoryStream(data));
            if (codec == null)
            {
                this.logger.LogInformation($"Cover of {data.Length:N0} bytes refused: not an image Skia can decode.");
                return null;
            }
            // A small file can claim enormous dimensions, and decoding allocates for all of them. A 48 MP phone photo is
            // the biggest real cover expected.
            if ((long)codec.Info.Width * codec.Info.Height > MaxInputPixels)
            {
                this.logger.LogInformation($"Cover of {data.Length:N0} bytes refused: {codec.Info.Width}x{codec.Info.Height} is too many pixels.");
                return null;
            }
            using SKBitmap source = SKBitmap.Decode(codec);
            if (source == null)
            {
                this.logger.LogInformation($"Cover of {data.Length:N0} bytes refused: {codec.EncodedFormat} failed to decode.");
                return null;
            }

            Prepared prepared = new(Draw(source, codec.EncodedOrigin, CoverMaxEdge), Draw(source, codec.EncodedOrigin, ThumbnailMaxEdge));
            this.logger.LogInformation($"Cover prepared from {data.Length:N0} bytes of {codec.EncodedFormat} {source.Width}x{source.Height} ({codec.EncodedOrigin}): cover {prepared.Cover.Length:N0} bytes, thumbnail {prepared.Thumbnail.Length:N0} bytes, in {sw.ElapsedMilliseconds}ms.");
            return prepared;
        }

        /// <summary>
        /// Scales <paramref name="source"/> to fit <paramref name="maxEdge"/> (never up), applies the orientation, and encodes
        /// a WebP on white. WebP could keep transparency, but a transparent cover with dark artwork disappears against the
        /// dark theme.
        /// </summary>
        private static byte[] Draw(SKBitmap source, SKEncodedOrigin origin, int maxEdge)
        {
            float scale = Math.Min(1f, maxEdge / (float)Math.Max(source.Width, source.Height));
            int scaledWidth = Math.Max(1, (int)Math.Round(source.Width * scale));
            int scaledHeight = Math.Max(1, (int)Math.Round(source.Height * scale));
            bool swapsAxes = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
            int width = swapsAxes ? scaledHeight : scaledWidth;
            int height = swapsAxes ? scaledWidth : scaledHeight;

            using SKBitmap target = new(width, height);
            using (SKCanvas canvas = new(target))
            {
                canvas.Clear(SKColors.White);
                // Canvas transforms apply to what's drawn in reverse order of the calls.
                switch (origin)
                {
                    case SKEncodedOrigin.TopRight:
                        canvas.Translate(width, 0);
                        canvas.Scale(-1, 1);
                        break;
                    case SKEncodedOrigin.BottomRight:
                        canvas.Translate(width, height);
                        canvas.RotateDegrees(180);
                        break;
                    case SKEncodedOrigin.BottomLeft:
                        canvas.Translate(0, height);
                        canvas.Scale(1, -1);
                        break;
                    case SKEncodedOrigin.LeftTop:
                        canvas.RotateDegrees(90);
                        canvas.Scale(1, -1);
                        break;
                    case SKEncodedOrigin.RightTop:
                        canvas.Translate(width, 0);
                        canvas.RotateDegrees(90);
                        break;
                    case SKEncodedOrigin.RightBottom:
                        canvas.Translate(width, height);
                        canvas.RotateDegrees(-90);
                        canvas.Scale(1, -1);
                        break;
                    case SKEncodedOrigin.LeftBottom:
                        canvas.Translate(0, height);
                        canvas.RotateDegrees(-90);
                        break;
                }
                using SKImage image = SKImage.FromBitmap(source);
                canvas.DrawImage(image, SKRect.Create(scaledWidth, scaledHeight), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            }

            using SKData encoded = target.Encode(SKEncodedImageFormat.Webp, Quality)
                ?? throw new InvalidOperationException("Skia could not encode WebP; its native library may have been built without it.");
            return encoded.ToArray();
        }
    }
}
