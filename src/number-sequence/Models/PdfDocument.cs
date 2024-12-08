using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace number_sequence.Models
{
    public sealed class PdfDocument
    {
        [Required]
        [MaxLength(64)]
        public string Id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset? ProcessedAt { get; set; }

        public bool? Successful { get; set; }
    }
}
