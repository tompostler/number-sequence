using System;
using System.ComponentModel.DataAnnotations;

namespace TcpWtf.NumberSequence.Models
{
    /// <summary>
    /// An account is used for authentication, data partitioning, and throttling limits.
    /// </summary>
    public sealed class Account
    {
        /// <summary>
        /// The name you wish to be associated with the account. This is the primary partitioning key.
        /// </summary>
        [Required, MaxLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// When the account was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// When the account was last modified.
        /// </summary>
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
