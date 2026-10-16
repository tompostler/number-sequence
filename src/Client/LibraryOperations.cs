namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Library operations.
    /// </summary>
    public sealed partial class LibraryOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal LibraryOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }
    }
}
