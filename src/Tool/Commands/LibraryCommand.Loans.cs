using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Libraries;
using Unlimitedinf.Utilities;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LibraryCommand
    {
        private static async Task HandleLoanListAsync(long? libraryId, bool all, int skip, int take, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            List<LibraryLoan> loans = await client.Library.GetLoansAsync(id, open: !all, skip, take);
            Console.WriteLine();
            if (loans.Count == 0)
            {
                Console.WriteLine(all ? "Nothing has been lent." : "Nothing is lent out.");
                return;
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            Output.WriteTable(
                loans.Select(x => new
                {
                    LoanId = x.Id,
                    x.CopyId,
                    x.Copy.Item.Title,
                    x.Copy.Format,
                    Borrower = x.BorrowerAccountName == null ? x.BorrowerName : $"{x.BorrowerName} ({x.BorrowerAccountName})",
                    Loaned = x.LoanedDate.ToString("yyyy-MM-dd"),
                    Due = x.DueDate.HasValue ? $"{x.DueDate:yyyy-MM-dd}{(x.ReturnedDate == null && x.DueDate < today ? " (overdue)" : string.Empty)}" : null,
                    Returned = x.ReturnedDate?.ToString("yyyy-MM-dd"),
                }),
                all
                    ? ["LoanId", "CopyId", "Title", "Format", "Borrower", "Loaned", "Due", "Returned"]
                    : ["LoanId", "CopyId", "Title", "Format", "Borrower", "Loaned", "Due"]);
        }
    }
}
