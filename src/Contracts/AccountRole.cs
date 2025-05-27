namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// The roles that can be assigned for an account. This will determine which feature are available to a specific account.
    /// </summary>
    public static class AccountRoles
    {
        /// <summary>
        /// Ability to view the status of the pdf document generation.
        /// </summary>
        public const string PdfStatus = nameof(PdfStatus);

        /// <summary>
        /// Verify that roles are working by being granted this role and using the ping endpoint.
        /// </summary>
        public const string Ping = nameof(Ping);
    }
}
