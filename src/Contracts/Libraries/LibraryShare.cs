using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcpWtf.NumberSequence.Contracts.Libraries
{
    /// <summary>
    /// Grants another account access to a <see cref="Libraries.Library"/>. That account also needs the
    /// <see cref="AccountRoles.Library"/> role.
    /// </summary>
    public sealed class LibraryShare
    {
        /// <summary>
        /// See <see cref="Library.Id"/>.
        /// </summary>
        public long LibraryId { get; set; }

        /// <summary>
        /// The account being granted access. See <see cref="Account.Name"/>.
        /// </summary>
        [Required, MaxLength(64)]
        public string AccountName { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The access granted. <see cref="LibraryPermission.Owner"/> is not valid here.
        /// </summary>
        [Required, Column(TypeName = "NVARCHAR(8)")]
        public LibraryPermission Permission { get; set; }

        /// <summary>
        /// The date the share was created.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date the share was modified.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset ModifiedDate { get; set; }


        /// <summary>
        /// The library being shared.
        /// </summary>
        public Library Library { get; set; }
    }
}
