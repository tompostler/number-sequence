using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts.Invoicing
{
    /// <summary>
    /// The acutal line items on the invoice.
    /// </summary>
    public sealed class InvoiceLine
    {
        /// <summary>
        /// The id of the line. Unique in the system.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The title of the line to be displayed in the row.
        /// </summary>
        [Required, MaxLength(128)]
        public string Title { get; set; }

        /// <summary>
        /// And option description of the line to be displayed beneath the title.
        /// </summary>
        [MaxLength(512)]
        public string Description { get; set; }

        /// <summary>
        /// The number of units.
        /// </summary>
        [Required]
        public decimal Quantity { get; set; }

        /// <summary>
        /// If not supplied, is "per unit" by default.
        /// Otherwise will be suffixed on the invoice.
        /// </summary>
        [MaxLength(8)]
        public string Unit { get; set; }

        /// <summary>
        /// The price per unit.
        /// </summary>
        [Required]
        public decimal Price { get; set; }

        /// <summary>
        /// The total for this line item.
        /// </summary>
        [NotMapped]
        public decimal Total => this.Quantity * this.Price;

        /// <summary>
        /// The date the line was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date the line was modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }


        /// <summary>
        /// A reference to the invoice this line is for.
        /// </summary>
        public Invoice Invoice { get; set; }
    }
}
