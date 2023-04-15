using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts.Framework;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Client for communciating with the REST APIs.
    /// </summary>
    public sealed class NsTcpWtfClient : IDisposable
    {
        private readonly ILogger<NsTcpWtfClient> logger;
        private readonly Func<CancellationToken, Task<string>> tokenCallback;
        private readonly HttpClient httpClient;

        private readonly string clientVersion;
        private readonly string clientName;

        /// <summary>
        /// Create a client.
        /// </summary>
        /// <param name="logger">Logger for any client logging. Logs requests and responses.</param>
        /// <param name="tokenCallback">A callback that will be invoked before any APIs requiring auth.</param>
        /// <param name="stamp">Override the base uri for unit testing or other hosting.</param>
        public NsTcpWtfClient(
            ILogger<NsTcpWtfClient> logger,
            Func<CancellationToken, Task<string>> tokenCallback,
            Stamp stamp = Stamp.Public)
        {
            this.logger = logger;
            this.tokenCallback = tokenCallback;

            Uri baseUri = stamp switch
            {
                Stamp.LocalDev => new Uri("http://localhost:44320/"),
                Stamp.Public => new Uri("https://ns.tcp.wtf/"),
                _ => throw new ArgumentOutOfRangeException(nameof(stamp))
            };

            this.httpClient = new HttpClient() { BaseAddress = baseUri };
            if (stamp == Stamp.LocalDev)
            {
                this.httpClient = new HttpClient(new HttpClientHandler() { ServerCertificateCustomValidationCallback = (_, _, _, _) => true }) { BaseAddress = baseUri };
            }

            this.clientVersion = Assembly.GetAssembly(typeof(NsTcpWtfClient))?.GetName()?.Version?.ToString(fieldCount: 3) ?? "0.0.0";
            this.clientName = Environment.MachineName;

            this.Account = new AccountOperations(this);
            this.Count = new CountOperations(this);
            this.Invoice = new InvoiceOperations(this);
            this.LatexStatus = new LatexStatusOperations(this);
            this.Ping = new PingOperations(this);
            this.Random = new RandomOperations(this);
            this.Token = new TokenOperations(this);
        }

        /// <summary>
        /// Account operations.
        /// </summary>
        public AccountOperations Account { get; }

        /// <summary>
        /// Count operations.
        /// </summary>
        public CountOperations Count { get; }

        /// <summary>
        /// Invoice operations.
        /// </summary>
        public InvoiceOperations Invoice { get; }

        /// <summary>
        /// Latex status operations.
        /// </summary>
        public LatexStatusOperations LatexStatus { get; }

        /// <summary>
        /// Ping operations.
        /// </summary>
        public PingOperations Ping { get; }

        /// <summary>
        /// Random operations.
        /// </summary>
        public RandomOperations Random { get; }

        /// <summary>
        /// Token operations.
        /// </summary>
        public TokenOperations Token { get; }

        /// <summary>
        /// Send a <see cref="HttpRequestMessage"/> with retries that are appropriate.
        /// Attempts to prepare the request by default.
        /// </summary>
        /// <remarks>
        /// We could evaluate using a framework, like Polly (which is netstandard available), but this works for now.
        /// </remarks>
        /// <param name="requestFactory">Have to generate the request every time, to ensure we don't encounter a "message was already sent" exception.</param>
        /// <param name="cancellationToken">Caller's cancellation token.</param>
        /// <param name="needsPreparation">If true, will attempt to add a token with the token callback before sending.</param>
        internal async Task<HttpResponseMessage> SendRequestAsync(
            Func<HttpRequestMessage> requestFactory,
            CancellationToken cancellationToken,
            bool needsPreparation = true)
        {
            const int maxTryNum = 5;

            HttpRequestMessage requestMessage = null;
            HttpResponseMessage responseMessage = null;
            var stopwatch = Stopwatch.StartNew();
            for (int tryNum = 1; tryNum <= maxTryNum; tryNum++)
            {
                try
                {
                    // Prepare and send the request
                    requestMessage = requestFactory();
                    if (needsPreparation && this.tokenCallback != default)
                    {
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Token", await this.tokenCallback(cancellationToken));
                    }
                    requestMessage.Headers.Add(HttpHeaderNames.ClientVersion, this.clientVersion);
                    requestMessage.Headers.Add(HttpHeaderNames.ClientName, this.clientName);
                    responseMessage = await this.httpClient.SendAsync(requestMessage, cancellationToken);

                    // Log any information based on the response
                    string serverVersionInfo = string.Empty;
                    if (responseMessage.Headers.TryGetValues(HttpHeaderNames.ServerVersion, out IEnumerable<string> serverVersionHeaders))
                    {
                        serverVersionInfo = $" (NS {serverVersionHeaders.FirstOrDefault()})";

                        // In case there's a newer version of the server available (which would mean there's a newer client)
                        if (serverVersionHeaders.Any())
                        {
                            const string localBuildVersion = "1.0.0";
                            string serverVersion = serverVersionHeaders.First();
                            if (!string.Equals(this.clientVersion, serverVersion, StringComparison.OrdinalIgnoreCase)
                                && !string.Equals(this.clientVersion, localBuildVersion, StringComparison.OrdinalIgnoreCase))
                            {
                                this.logger.LogWarning($"Current client version is {this.clientVersion} but server version ({serverVersion}) indicates there's a newer version available. If using as a tool, update with 'dotnet tool update TcpWtf.NumberSequence.Tool --global'");
                            }
                        }
                    }

                    this.logger.LogInformation($"Received {responseMessage.StatusCode} from {requestMessage.Method} {requestMessage.RequestUri}{serverVersionInfo} after {stopwatch.ElapsedMilliseconds}ms");
                    
                    if (responseMessage.Headers.TryGetValues(HttpHeaderNames.ApiDeprecated, out IEnumerable<string> apiDeprecatedHeaders)
                        && DateTime.TryParseExact(apiDeprecatedHeaders.FirstOrDefault(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime apiDeprecatedAt))
                    {
                        if (apiDeprecatedAt < DateTime.UtcNow)
                        {
                            this.logger.LogError($"This API was obsoleted {apiDeprecatedAt:yyyy-MM}! Update your client!");
                        }
                        else
                        {
                            this.logger.LogWarning($"This API is deprecated with planned obsolescence {apiDeprecatedAt:yyyy-MM}. Update your client to use the latest APIs.");
                        }
                    }
                }
                catch (Exception ex) when (tryNum < maxTryNum && !cancellationToken.IsCancellationRequested)
                {
                    // If we've caught an unexpected exception when trying to call the service where there are still attempts remaining, then delay and try again
                    this.logger.LogWarning(ex.ToString());
                    await this.DelayBetweenAttemptsAsync(tryNum, "exception", cancellationToken);
                    continue;
                }

                // If the response is retryable (5xx) and we haven't exceeded our attempt limit yet, then delay and try again
                if ((int)responseMessage.StatusCode >= 500
                    && (int)responseMessage.StatusCode <= 600
                    && tryNum < maxTryNum)
                {
                    await this.DelayBetweenAttemptsAsync(tryNum, $"{responseMessage.StatusCode} status code", cancellationToken);
                    continue;
                }

                // If the response was successful then return it
                if (responseMessage.IsSuccessStatusCode)
                {
                    return responseMessage;
                }

                // Else the response was not successful (and should not be retried). Default behavior is to throw a client-specific exception
                string msg = $"Request returned an invalid status code '{responseMessage.StatusCode}'";

                // Attempt to read the body, but reset the stream position in case a subsequent user of the responseMessage wants to read it too
                try
                {
                    await responseMessage.Content.LoadIntoBufferAsync();
                    string body = await responseMessage.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        msg += $" Body:{Environment.NewLine}{body}";
                    }

                    Stream contentStream = await responseMessage.Content.ReadAsStreamAsync();
                    contentStream.Position = 0;
                }
                catch
                { }

                // Only log 5xx as error so we don't pollute telemetry with potentially acceptable errors
                if ((int)responseMessage.StatusCode < 500)
                {
                    this.logger.LogWarning(msg);
                }
                else
                {
                    this.logger.LogError(msg);
                }

                throw new NsTcpWtfClientException(msg)
                {
                    Request = requestMessage,
                    Response = responseMessage
                };
            }

            // We're only down here if we left the retry loop which means we exceeded our retries
            // Note that the responseMessage may be null if we were never able to reach the service
            throw new NsTcpWtfClientException($"Maximum retries when communicating exceeded after {stopwatch.Elapsed}")
            {
                Request = requestMessage,
                Response = responseMessage
            };
        }

        private async Task DelayBetweenAttemptsAsync(int tryCount, string failureReason, CancellationToken cancellationToken)
        {
            const int baseDelaySeconds = 2;
            const double delayExponent = 1.8;

            // This will retry 2s delay, 3.6s delay, 6.5s delay, 11.7s delay, 21s delay
            // For a total of 44.8s of delay (assuming instant response received)
            var delay = TimeSpan.FromSeconds(Math.Pow(delayExponent, tryCount - 1) * baseDelaySeconds);
            this.logger.LogWarning($"Attempt {tryCount + 1} after a {delay} delay due to {failureReason}.");
            await Task.Delay(delay, cancellationToken);
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <inheritdoc/>
        private void Dispose(bool disposeManagedResources)
        {
            if (!this.disposedValue)
            {
                if (disposeManagedResources)
                {
                    this.httpClient.Dispose();
                }

                this.disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposeManagedResources) above.
            this.Dispose(true);
        }

        #endregion
    }
}
