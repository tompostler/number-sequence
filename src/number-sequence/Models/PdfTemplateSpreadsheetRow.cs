﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace number_sequence.Models
{
    public sealed class PdfTemplateSpreadsheetRow
    {
        [Required]
        [MaxLength(64)]
        public string SpreadsheetId { get; set; }

        [Required]
        [MaxLength(64)]
        public string RowId { get; set; }

        [Required]
        [MaxLength(64)]
        public string DocumentId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset? ProcessedAt { get; set; }
    }
}
