namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// Publicly-documented limits based on the tier for an account.
    /// </summary>
    public static class TierLimits
    {
        /// <summary>
        /// Number of accounts is based on the maximum value associated with the created from for existing accounts that match,
        /// else <see cref="AccountTier.Small"/> by default.
        /// </summary>
        public static readonly IReadOnlyDictionary<AccountTier, int> AccountsPerCreatedFrom = new Dictionary<AccountTier, int>
        {
            [AccountTier.Small] = 3,
            [AccountTier.Medium] = 7,
            [AccountTier.Large] = 19,
            [AccountTier.Infinite] = int.MaxValue,
        };

        /// <summary>
        /// Number of tokens allowed is based on the account tier.
        /// </summary>
        public static readonly IReadOnlyDictionary<AccountTier, int> TokensPerAccount = AccountsPerCreatedFrom;

        /// <summary>
        /// Number of counts allowed is based on the account tier.
        /// </summary>
        public static readonly IReadOnlyDictionary<AccountTier, int> CountsPerAccount = AccountsPerCreatedFrom;

        /// <summary>
        /// Number of 'days since' allowed is based on the account tier.
        /// </summary>
        public static readonly IReadOnlyDictionary<AccountTier, int> DaysSincePerAccount = AccountsPerCreatedFrom;

        /// <summary>
        /// Number of 'days since' events allowed per 'days since' is based on the account tier.
        /// </summary>
        public static readonly IReadOnlyDictionary<AccountTier, int> DaysSinceEventsPerEvent = new Dictionary<AccountTier, int>
        {
            [AccountTier.Small] = 16,
            [AccountTier.Medium] = 32,
            [AccountTier.Large] = 64,
            [AccountTier.Infinite] = int.MaxValue,
        };

        /// <summary>
        /// Number of redirects allowed is based on the account tier.
        /// </summary>
        public static readonly IReadOnlyDictionary<AccountTier, int> RedirectsPerAccount = AccountsPerCreatedFrom;

        /// <summary>
        /// Indicates how much priority cached items should have based on the account tier.
        /// </summary>
        public static readonly IReadOnlyDictionary<AccountTier, int> CacheItemPriority = new Dictionary<AccountTier, int>
        {
            [AccountTier.Small] = 0,
            [AccountTier.Medium] = 1,
            [AccountTier.Large] = 2,
            [AccountTier.Infinite] = 2,
        };
    }
}
