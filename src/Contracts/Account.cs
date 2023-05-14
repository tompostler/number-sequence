using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// An account is used for authentication, data partitioning, and throttling limits.
    /// </summary>
    public sealed class Account
    {
        /// <summary>
        /// The name you wish to be associated with the account. This is the primary partitioning key.
        /// </summary>
        [Required, MinLength(3), MaxLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// Effectively the account's password.
        /// Stored as a simple cryptographic hash in the backend since this is a personal project and doesn't need to go crazy with password protection.
        /// </summary>
        [Required, MinLength(32), MaxLength(128)]
        public string Key { get; set; }

        /// <summary>
        /// The tier for the account. Default is <see cref="AccountTier.Small"/>.
        /// </summary>
        [Required, Column(TypeName = "NVARCHAR(8)")]
        public AccountTier Tier { get; set; }

        /// <summary>
        /// When an account should have access to more features, it will have additional roles. See <see cref="AccountRoles"/>.
        /// </summary>
        [MaxLength(64)]
        public string Roles { get; set; }

        /// <summary>
        /// When the account was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// Some information that identifies where the account creation came from. Used to block spam.
        /// </summary>
        [Required, MaxLength(64)]
        public string CreatedFrom { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// When the account was last modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }
    }
}
