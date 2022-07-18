using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
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
    public sealed class NsTcpWtfClient
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
                Stamp.LocalDev => new Uri("https://localhost:44321/"),
                Stamp.Public => new Uri("https://ns.tcp.wtf/"),
                _ => throw new ArgumentOutOfRangeException(nameof(stamp))
            };
            if (stamp == Stamp.LocalDev)
            {
                this.httpClient = new HttpClient(new HttpClientHandler() { ServerCertificateCustomValidationCallback = (_, _, _, _) => true }) { BaseAddress = baseUri };
            }
            else
            {
                this.httpClient = new HttpClient() { BaseAddress = baseUri };
            }

            this.clientVersion = Assembly.GetAssembly(typeof(NsTcpWtfClient)).GetName().Version.ToString(3);
            this.clientName = Environment.MachineName;

            this.Account = new AccountOperations(this);
            this.Count = new CountOperations(this);
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
        /// Does not prepare the request.
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
            const int baseDelaySeconds = 2;
            const double delayExponent = 1.7;

            HttpRequestMessage request = null;
            HttpResponseMessage response = null;
            var stopwatch = Stopwatch.StartNew();
            for (int tryNum = 0; tryNum <= maxTryNum; tryNum++)
            {
                // Prepare and send the request
                request = requestFactory();
                if (needsPreparation && this.tokenCallback != default)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Token", await this.tokenCallback(cancellationToken));
                }
                request.Headers.Add(HttpHeaderNames.ClientVersion, this.clientVersion);
                request.Headers.Add(HttpHeaderNames.ClientName, this.clientName);
                response = await this.httpClient.SendAsync(request, cancellationToken);

                // Log any information based on the response
                string serverVersionInfo = string.Empty;
                if (response.Headers.TryGetValues(HttpHeaderNames.ServerVersion, out System.Collections.Generic.IEnumerable<string> serverVersionHeaders))
                {
                    serverVersionInfo = $" (NS {serverVersionHeaders.FirstOrDefault()})";
                }
                this.logger.LogInformation("Received {0} from {1} {2}{3}", response.StatusCode, request.Method, request.RequestUri, serverVersionInfo);
                if (response.Headers.TryGetValues(HttpHeaderNames.ApiDeprecated, out System.Collections.Generic.IEnumerable<string> apiDeprecatedHeaders)
                    && DateTime.TryParseExact(apiDeprecatedHeaders.FirstOrDefault(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime apiDeprecatedAt))
                {
                    if (apiDeprecatedAt < DateTime.UtcNow)
                    {
                        this.logger.LogError("This API was obsoleted {0:yyyy-MM}! Update your client!", apiDeprecatedAt);
                    }
                    else
                    {
                        this.logger.LogWarning("This API is deprecated with planned obsolescence {0:yyyy-MM}. Update your client to use the latest APIs.", apiDeprecatedAt);
                    }
                }

                // Handle the response
                if (!response.IsSuccessStatusCode && !((int)response.StatusCode >= 400 && (int)response.StatusCode < 500))
                {
                    // We only want to delay if it's not the last try. Otherwise, fall out and throw.
                    if (tryNum < maxTryNum)
                    {
                        // When deploying, it can take 15-30 seconds for the app service to fully recycle
                        // This will retry immediately, 3.4s delay, 5.8s delay, 9.8s delay, 16.7s delay
                        // For a total of 35.7s of delay (assuming instant response received)
                        var delay = TimeSpan.FromSeconds(Math.Pow(delayExponent, tryNum) * baseDelaySeconds);
                        this.logger.LogWarning("Attempt {0} after a {1} delay due to {2} status code.", tryNum + 1, delay, response.StatusCode);
                        await Task.Delay(delay, cancellationToken);
                    }
                }
                else if (!response.IsSuccessStatusCode)
                {
                    string msg = $"Operation returned an invalid status code [{response.StatusCode}]";

                    // Attempt to read the body
                    try
                    {
                        string body = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            msg += $"{Environment.NewLine}{body}";
                        }
                    }
                    catch (Exception)
                    { }

                    // Only log 5xx as error so we don't pollute telemetry with potentially acceptable errors
                    if ((int)response.StatusCode < 500)
                    {
                        this.logger.LogWarning(msg);
                    }
                    else
                    {
                        this.logger.LogError(msg);
                    }

                    throw new NsTcpWtfClientException(msg)
                    {
                        Request = request,
                        Response = response
                    };
                }
                else
                {
                    return response;
                }
            }

            // We're only down here if we exceeded our retries. This should be very rare.
            throw new NsTcpWtfClientException($"Maximum retries when communicating exceeded after {stopwatch.Elapsed}")
            {
                Request = request,
                Response = response
            };
        }

    }
}
