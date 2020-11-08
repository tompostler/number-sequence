using System;
using System.ComponentModel.DataAnnotations;
using TcpWtf.NumberSequence.Contracts.Interfaces;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <inheritdoc/>
    public sealed class Count : ICount
    {
        /// <inheritdoc/>
        public string Account { get; set; }

        /// <inheritdoc/>
        [Required, MinLength(3), MaxLength(64)]
        public string Name { get; set; }

        /// <inheritdoc/>
        public ulong Value { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset CreatedAt { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
