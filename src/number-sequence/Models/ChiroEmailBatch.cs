using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace number_sequence.Models
{
    public sealed class ChiroEmailBatch
    {
        [Required]
        [MaxLength(64)]
        public string Id { get; set; }

        [Required]
        [MaxLength(16)]
        public string ClinicAbbreviation { get; set; }

        [MaxLength(128)]
        public string AttachmentName { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset? ProcessedAt { get; set; }
    }
}
