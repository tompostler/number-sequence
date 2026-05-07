using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace number_sequence.Models
{
    public sealed class ChiroRecord
    {
        [Required]
        [MaxLength(64)]
        public string RowId { get; set; }

        [Required]
        [MaxLength(128)]
        public string Source { get; set; }

        public DateTimeOffset DataEnteredAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset RecordedAt { get; set; }

        public DateTimeOffset? ProcessedAt { get; set; }

        public string InputJson { get; set; }
    }
}
