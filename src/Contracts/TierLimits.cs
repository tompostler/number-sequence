using System.Collections.Generic;

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
            [AccountTier.Infinite] = int.MaxValue
        };
    }
}
