using System.Reflection;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// The roles that can be assigned for an account. This will determine which feature are available to a specific account.
    /// </summary>
    public static class AccountRoles
    {
        /// <summary>
        /// The account can list accounts and change any account's tier and roles.
        /// </summary>
        public const string Admin = nameof(Admin);

        /// <summary>
        /// The account has access to the chiro record entry forms.
        /// </summary>
        public const string Chiro = nameof(Chiro);

        /// <summary>
        /// The account has access to the ledger features.
        /// </summary>
        public const string Ledger = nameof(Ledger);

        /// <summary>
        /// The account has access to the library features, including libraries shared with it by other accounts.
        /// </summary>
        public const string Library = nameof(Library);

        /// <summary>
        /// Ability to view the status of the pdf document generation.
        /// </summary>
        public const string PdfStatus = nameof(PdfStatus);

        /// <summary>
        /// Verify that roles are working by being granted this role and using the ping endpoint.
        /// </summary>
        public const string Ping = nameof(Ping);

        /// <summary>
        /// Every role defined above, read from the constants themselves so there is no second list to keep in sync.
        /// </summary>
        public static readonly IReadOnlyList<string> All = typeof(AccountRoles)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.IsLiteral && x.FieldType == typeof(string))
            .Select(x => (string)x.GetRawConstantValue())
            .OrderBy(x => x)
            .ToList();

        /// <summary>
        /// Split the stored <see cref="Account.Roles"/> value into its distinct roles.
        /// </summary>
        public static List<string> Parse(string roles)
            => (roles ?? string.Empty)
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }
}
