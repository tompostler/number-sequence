﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace number_sequence.Models
{
    public sealed class EmailDocument
    {
        [Required]
        [MaxLength(64)]
        public string Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string To { get; set; }

        [MaxLength(128)]
        public string CC { get; set; }

        [Required]
        [MaxLength(128)]
        public string Subject { get; set; }

        [MaxLength(128)]
        public string AttachmentName { get; set; }

        [MaxLength(512)]
        public string AdditionalBody { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset? ProcessedAt { get; set; }
    }
}
