using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts.Invoicing
{
    /// <summary>
    /// Used for defining standard/default <see cref="InvoiceLine"/> items.
    /// </summary>
    public sealed class InvoiceLineDefault
    {
        /// <summary>
        /// See <see cref="Account.Name"/>.
        /// </summary>
        [Required, MaxLength(64)]
        public string AccountName { get; set; }

        /// <summary>
        /// The id of the line default. Unique in the system.
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
        /// The date the line default was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date the line default was modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }

        /// <summary>
        /// Enables conversion from <see cref="InvoiceLineDefault"/> to <see cref="InvoiceLine"/>.
        /// </summary>
        public static implicit operator InvoiceLine(InvoiceLineDefault @this)
        {
            if (@this is null)
            {
                return null;
            }

            return new()
            {
                Id = @this.Id,
                Title = @this.Title,
                Description = @this.Description,
                Quantity = @this.Quantity,
                Unit = @this.Unit,
                Price = @this.Price
            };
        }
    }
}
