using System;
using System.ComponentModel.DataAnnotations;

namespace TcpWtf.NumberSequence.Contracts.Interfaces
{
    /// <summary>
    /// A token is used for actual authentication.
    /// </summary>
    public interface IToken
    {
        /// <summary>
        /// The name of the account the token is generated for.
        /// </summary>
        [Required, MinLength(3), MaxLength(64)]
        public string Account { get; set; }

        /// <summary>
        /// The account's password. Used when creating a token to verify ownership.
        /// </summary>
        [Required, MinLength(32), MaxLength(128)]
        public string Key { get; set; }

        /// <summary>
        /// The name you wish to be associated with the token. This allows you to have more than one token.
        /// </summary>
        [Required, MinLength(3), MaxLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// The actual token value to be used on authentication calls.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// When the token was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// When the token expires. Default is 90 days.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
