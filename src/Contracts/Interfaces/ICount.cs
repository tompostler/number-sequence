using System;
using System.ComponentModel.DataAnnotations;

namespace TcpWtf.NumberSequence.Contracts.Interfaces
{
    /// <summary>
    /// Keep track of a count, ulong.
    /// </summary>
    public interface ICount
    {
        /// <summary>
        /// The name of the account the count is for. Populated from the token used for authentication.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// The name you wish to be associated with the count. This allows you to have more than one count.
        /// </summary>
        [Required, MinLength(3), MaxLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// The actual count value.
        /// </summary>
        public ulong Value { get; set; }

        /// <summary>
        /// When the count was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// When the count was last modified.
        /// </summary>
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
