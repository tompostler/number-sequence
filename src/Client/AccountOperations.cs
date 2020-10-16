namespace TcpWtf.NumberSequence.Client
{
    public sealed class AccountOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal AccountOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }
    }
}