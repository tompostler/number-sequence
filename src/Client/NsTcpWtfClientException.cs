using System;
using System.Net.Http;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Thrown when a failure occurs with the client.
    /// </summary>
    [Serializable]
    public sealed class NsTcpWtfClientException : Exception
    {
        /// <summary>
        /// The request sent to the server. May be null depending on the requestor.
        /// </summary>
        public HttpRequestMessage Request { get; set; }

        /// <summary>
        /// The response received from the server.
        /// </summary>
        public HttpResponseMessage Response { get; set; }

        /// <inheritdoc/>
        public NsTcpWtfClientException()
        { }

        /// <inheritdoc/>
        public NsTcpWtfClientException(string message) : base(message)
        { }

        /// <inheritdoc/>
        public NsTcpWtfClientException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}
