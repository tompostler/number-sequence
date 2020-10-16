namespace TcpWtf.NumberSequence.Contracts.Framework
{
    /// <summary>
    /// Names of HTTP headers.
    /// </summary>
    public static class HttpHeaderNames
    {
        /// <summary>
        /// The version of the client.
        /// </summary>
        public const string ClientVersion = "client-version";

        /// <summary>
        /// The name of the machine running the client.
        /// </summary>
        public const string ClientName = "client-name";

        /// <summary>
        /// The version of the server.
        /// Logged by the client.
        /// </summary>
        public const string ServerVersion = "server-version";

        /// <summary>
        /// If an API is deprecated, this will be set with a yyyy-MM of the planned API removal.
        /// Logged as a warning or error based on the date by the client.
        /// </summary>
        /// <remarks>
        /// While using the <see cref="System.ObsoleteAttribute"/> works if clients are actively taking newer packages,
        /// this will enable an old client to start seeing deprecation warnings from a newer server.
        /// </remarks>
        public const string ApiDeprecated = "api-deprecated";
    }
}
