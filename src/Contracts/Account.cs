using System;
using System.ComponentModel.DataAnnotations;
using TcpWtf.NumberSequence.Contracts.Interfaces;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <inheritdoc/>
    public sealed class Account : IAccount
    {
        /// <inheritdoc/>
        [Required, MaxLength(64)]
        public string Name { get; set; }

        /// <inheritdoc/>
        [Required, MinLength(32), MaxLength(128)]
        public string Key { get; set; }

        /// <inheritdoc/>
        public AccountTier Tier { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset CreatedAt { get; set; }

        /// <inheritdoc/>
        public string CreatedFrom { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
