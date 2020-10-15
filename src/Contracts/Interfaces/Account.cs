using System;
using System.ComponentModel.DataAnnotations;

namespace TcpWtf.NumberSequence.Contracts.Interfaces
{
    /// <summary>
    /// An account is used for authentication, data partitioning, and throttling limits.
    /// </summary>
    public interface IAccount
    {
        /// <summary>
        /// The name you wish to be associated with the account. This is the primary partitioning key.
        /// </summary>
        [Required, MaxLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// Effectively the account's password.
        /// Stored as a simple cryptographic hash in the backend since this is a personal project and doesn't need to go crazy with password
        /// protection.
        /// </summary>
        [Required, MinLength(32), MaxLength(128)]
        public string Key { get; set; }

        /// <summary>
        /// The tier for the account. Default is <see cref="AccountTier.Small"/>.
        /// </summary>
        public AccountTier Tier { get; set; }

        /// <summary>
        /// When the account was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Some information that identifies where the account creation came from. Used to block spam.
        /// </summary>
        public string CreatedFrom { get; set; }

        /// <summary>
        /// When the account was last modified.
        /// </summary>
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
