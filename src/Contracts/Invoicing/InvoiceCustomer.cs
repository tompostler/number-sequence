using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TcpWtf.NumberSequence.Contracts.Invoicing
{
    /// <summary>
    /// The customer being billed.
    /// </summary>
    public sealed class InvoiceCustomer
    {
        /// <summary>
        /// See <see cref="Account.Name"/>.
        /// </summary>
        [Required, MaxLength(64)]
        public string AccountName { get; set; }

        /// <summary>
        /// The id of the customer. Unique in the system.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The customer name.
        /// </summary>
        [Required, MaxLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// The customer address.
        /// </summary>
        [Required, MaxLength(64)]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// The customer address.
        /// </summary>
        [Required, MaxLength(64)]
        public string AddressLine2 { get; set; }

        /// <summary>
        /// Contact information for the customer, such as email or phone.
        /// </summary>
        [Required, MaxLength(64)]
        public string Contact { get; set; }

        /// <summary>
        /// The date the customer was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date the customer was modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }


        /// <summary>
        /// A reference to the invoices this customer has in the system.
        /// </summary>
        public ICollection<Invoice> Invoices { get; set; }
    }
}
