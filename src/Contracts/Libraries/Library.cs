using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// A collection of physical media owned by one account, optionally shared with others via <see cref="LibraryShare"/>.
    /// </summary>
    public sealed class Library
    {
        /// <summary>
        /// The id of the library. Unique in the system.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The owning account. See <see cref="Account.Name"/>.
        /// </summary>
        [Required, MaxLength(64)]
        public string AccountName { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The library name, such as "Home" or "Cabin".
        /// </summary>
        [Required, MaxLength(64)]
        public string Name { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// An optional description.
        /// </summary>
        [MaxLength(256)]
        public string Description { get; set; }

        /// <summary>
        /// The calling account's access to this library. Computed per request, not stored.
        /// </summary>
        [NotMapped]
        public LibraryPermission CallerPermission { get; set; }

        /// <summary>
        /// The date the library was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date the library was modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }


        /// <summary>
        /// The accounts this library is shared with.
        /// </summary>
        public ICollection<LibraryShare> Shares { get; set; }
    }
}
