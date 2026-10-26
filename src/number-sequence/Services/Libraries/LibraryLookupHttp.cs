using number_sequence.Exceptions;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace number_sequence.Services.Libraries
{
    /// <summary>
    /// Sends one request to a lookup provider and logs it. Every way the request can fail becomes a
    /// <see cref="FailedDependencyException"/> naming the provider, so the add page can say which service is down and carry
    /// on with typing details in. A 404 isn't a failure: it means the provider has no such record, and returns null.
    /// </summary>
    public sealed class LibraryLookupHttp
    {
        public const string HttpClientName = "library-lookup";

        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<LibraryLookupHttp> logger;

        public LibraryLookupHttp(IHttpClientFactory httpClientFactory, ILogger<LibraryLookupHttp> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        /// <summary>
        /// The response body, or null on a 404.
        /// </summary>
        public async Task<string> GetStringAsync(string provider, string url, CancellationToken cancellationToken, string bearerToken = null)
        {
            Stopwatch sw = Stopwatch.StartNew();
            using HttpRequestMessage request = new(HttpMethod.Get, url);
            if (bearerToken != null)
            {
                request.Headers.Authorization = new("Bearer", bearerToken);
            }

            HttpResponseMessage response;
            try
            {
                response = await this.httpClientFactory.CreateClient(HttpClientName).SendAsync(request, cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
            {
                string reason = ex is TaskCanceledException ? "timed out" : ex.Message;
                this.logger.LogInformation($"{provider} GET {url} failed after {sw.ElapsedMilliseconds}ms: {reason}");
                throw new FailedDependencyException($"{provider} didn't answer ({reason}).", ex);
            }

            using (response)
            {
                string body = await response.Content.ReadAsStringAsync(cancellationToken);
                this.logger.LogInformation($"{provider} GET {url} returned {(int)response.StatusCode} with {body.Length:N0} chars in {sw.ElapsedMilliseconds}ms.");

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    throw new FailedDependencyException($"{provider} refused the request ({(int)response.StatusCode}). Check its token.");
                }
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new FailedDependencyException($"{provider} is limiting how often it's asked. Try again in a minute.");
                }
                if (!response.IsSuccessStatusCode)
                {
                    throw new FailedDependencyException($"{provider} returned {(int)response.StatusCode}.");
                }
                return body;
            }
        }

        /// <summary>
        /// <see cref="GetStringAsync"/> parsed as json, or null on a 404. A body that isn't json is a provider failure.
        /// </summary>
        public async Task<JsonDocument> GetJsonAsync(string provider, string url, CancellationToken cancellationToken, string bearerToken = null)
        {
            string body = await this.GetStringAsync(provider, url, cancellationToken, bearerToken);
            if (body == null)
            {
                return null;
            }
            try
            {
                return JsonDocument.Parse(body);
            }
            catch (JsonException ex)
            {
                throw new FailedDependencyException($"{provider} returned something that isn't json.", ex);
            }
        }
    }
}
