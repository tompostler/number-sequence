using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace number_sequence.Models
{
    public sealed class LatexTemplate
    {
        [Required]
        [MaxLength(64)]
        public string Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string SpreadsheetId { get; set; }

        [Required]
        [MaxLength(16)]
        public string SpreadsheetRange { get; set; }

        [Required]
        [MaxLength(64)]
        public string EmailTo { get; set; }

        [MaxLength(128)]
        public string SubjectTemplate { get; set; }

        [MaxLength(128)]
        public string AttachmentNameTemplate { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }
    }
}
