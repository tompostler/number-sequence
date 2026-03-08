namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Ledger operations.
    /// </summary>
    public sealed partial class LedgerOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal LedgerOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }
    }
}
