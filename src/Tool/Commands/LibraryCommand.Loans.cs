using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Libraries;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

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

        private static async Task HandleOwnedAsync(string barcode, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            List<LibraryCopy> copies = await client.Library.GetCopiesByBarcodeAsync(barcode);
            if (raw)
            {
                Console.WriteLine(copies.ToJsonString(indented: true));
                return;
            }

            Console.WriteLine();
            if (copies.Count == 0)
            {
                Console.WriteLine($"Not owned: no copies with barcode [{barcode}] in any library you can access.");
                return;
            }

            Dictionary<long, string> libraryNames = (await client.Library.GetLibrariesAsync()).ToDictionary(x => x.Id, x => x.Name);
            Dictionary<long, Dictionary<long, string>> pathsByLibrary = [];
            foreach (long libraryId in copies.Select(x => x.LibraryId).Distinct())
            {
                pathsByLibrary[libraryId] = await GetLocationPathsAsync(client, libraryId);
            }

            Output.WriteTable(
                copies.Select(x => new
                {
                    Library = libraryNames.GetValueOrDefault(x.LibraryId, $"#{x.LibraryId}"),
                    x.ItemId,
                    x.Item.Title,
                    CopyId = x.Id,
                    x.Format,
                    Location = LocationPath(pathsByLibrary[x.LibraryId], x.LocationId),
                    x.Status,
                    OnLoanTo = x.Loans?.FirstOrDefault()?.BorrowerName,
                }),
                "Library",
                "ItemId",
                "Title",
                "CopyId",
                "Format",
                "Location",
                "Status",
                "OnLoanTo");
        }
    }
}
