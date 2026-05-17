using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace number_sequence.Models
{
    public sealed class ChiroEmailBatch
    {
        [MaxLength(64)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [MaxLength(16)]
        public string ClinicAbbreviation { get; set; }

        [MaxLength(64)]
        public string CcEmail { get; set; }

        [MaxLength(128)]
        public string AttachmentName { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset? ProcessedAt { get; set; }
    }
}
