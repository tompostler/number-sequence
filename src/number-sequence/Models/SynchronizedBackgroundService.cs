using System;
using System.ComponentModel.DataAnnotations;

namespace number_sequence.Models
{
    public sealed class SynchronizedBackgroundService
    {
        [Required]
        [MaxLength(128)]
        public string Name { get; set; }

        public DateTimeOffset LastExecuted { get; set; }

        public ulong CountExecutions { get; set; }
    }
}
