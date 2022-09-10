using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace TcpWtf.NumberSequence.Contracts.Invoicing
{
    /// <summary>
    /// The business unit generating the invoice.
    /// </summary>
    public sealed class InvoiceBusiness
    {
        /// <summary>
        /// See <see cref="Account.Name"/>.
        /// </summary>
        [Required, MaxLength(64)]
        public string AccountName { get; set; }

        /// <summary>
        /// The id of the busines unit. Unique in the system.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The business unit name.
        /// </summary>
        [Required, MaxLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// The business unit address.
        /// </summary>
        [Required, MaxLength(64)]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// The business unit address.
        /// </summary>
        [Required, MaxLength(64)]
        public string AddressLine2 { get; set; }

        /// <summary>
        /// Contact information for the business unit, such as email or phone.
        /// </summary>
        [Required, MaxLength(64)]
        public string Contact { get; set; }

        /// <summary>
        /// The date the business unit was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date the business unit was modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }


        /// <summary>
        /// A reference to the invoices this business unit has in the system.
        /// </summary>
        public ICollection<Invoice> Invoices { get; set; }
    }
}
