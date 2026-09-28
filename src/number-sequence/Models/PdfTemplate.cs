using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace number_sequence.Models
{
    public sealed class PdfTemplate
    {
        [Required]
        [MaxLength(64)]
        public string Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string EmailTo { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }
    }
}
