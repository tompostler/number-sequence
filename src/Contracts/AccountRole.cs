namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// The roles that can be assigned for an account. This will determine which feature are available to a specific account.
    /// </summary>
    public static class AccountRoles
    {
        /// <summary>
        /// The account has access to the invoicing features.
        /// </summary>
        public const string Invoicing = nameof(Invoicing);

        /// <summary>
        /// Ability to view the status of the latex document generation.
        /// </summary>
        public const string LatexStatus = nameof(LatexStatus);

        /// <summary>
        /// Verify that roles are working by being granted this role and using the ping endpoint.
        /// </summary>
        public const string Ping = nameof(Ping);
    }
}
