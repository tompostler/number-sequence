using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Libraries;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LibraryCommand
    {
        private static async Task HandleCopyCreateAsync(long? libraryId, long itemId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            LibraryItem item = await client.Library.GetItemAsync(id, itemId);
            Console.WriteLine($"Adding a copy of [{item.Title}], which has {item.Copies.Count} already.");
            LibraryCopy copy = await PromptAndCreateCopyAsync(client, id, item, await client.Library.GetLocationsAsync(id));
            Console.WriteLine(copy.ToJsonString(indented: true));
        }

        private static async Task HandleCopyGetAsync(long? libraryId, long copyId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);
            LibraryCopy copy = await client.Library.GetCopyAsync(id, copyId);
            Console.WriteLine(copy.ToJsonString(indented: true));
        }

        private static async Task HandleCopyEditAsync(long? libraryId, long copyId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            LibraryCopy copy = await client.Library.GetCopyAsync(id, copyId);
            LibraryItem item = await client.Library.GetItemAsync(id, copy.ItemId);
            Console.WriteLine($"Copy [{copy.Id}] of [{item.Title}]. Location, status and loans have their own commands.");

            copy.ItemId = PromptValue($"{nameof(copy.ItemId)} (another item's id merges this copy into it)", (long?)copy.ItemId) ?? copy.ItemId;
            PromptCopyFields(copy, item.MediaType);

            copy = await client.Library.UpdateCopyAsync(id, copy);
            Console.WriteLine(copy.ToJsonString(indented: true));
        }

        private static async Task HandleCopyDeleteAsync(long? libraryId, long copyId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            if (!Input.GetBool($"Delete copy [{copyId}] with its loans and history? To keep the history of a copy that's gone, mark it {LibraryCopyStatus.Disposed} instead"))
            {
                Console.WriteLine("Not deleted.");
                return;
            }

            await client.Library.DeleteCopyAsync(id, copyId);
            Console.WriteLine($"Deleted copy [{copyId}].");
        }

        private static async Task HandleCopyMoveAsync(long? libraryId, long copyId, string notes, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            LibraryCopy copy = await client.Library.GetCopyAsync(id, copyId);
            WriteLocations(await client.Library.GetLocationsAsync(id));
            LibraryCopyMove move = new()
            {
                LocationId = PromptValue(nameof(LibraryCopyMove.LocationId), copy.LocationId),
                Notes = notes,
            };

            copy = await client.Library.MoveCopyAsync(id, copyId, move);
            Console.WriteLine(copy.ToJsonString(indented: true));
        }

        private static async Task HandleCopyStatusAsync(long? libraryId, long copyId, LibraryCopyStatus status, string notes, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            LibraryCopy copy = await client.Library.SetCopyStatusAsync(id, copyId, new LibraryCopyStatusChange { Status = status, Notes = notes });
            Console.WriteLine(copy.ToJsonString(indented: true));
        }

        private static async Task HandleCopyLoanAsync(long? libraryId, long copyId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            LibraryLoan loan = new()
            {
                BorrowerName = PromptRequiredString(nameof(LibraryLoan.BorrowerName)),
                BorrowerAccountName = PromptString($"{nameof(LibraryLoan.BorrowerAccountName)} (if they have an account)"),
                LoanedDate = PromptValue(nameof(LibraryLoan.LoanedDate), (DateOnly?)today) ?? today,
                DueDate = PromptValue<DateOnly>(nameof(LibraryLoan.DueDate)),
                Notes = PromptString(nameof(LibraryLoan.Notes)),
            };

            loan = await client.Library.LoanCopyAsync(id, copyId, loan);
            Console.WriteLine(loan.ToJsonString(indented: true));
        }

        private static async Task HandleCopyReturnAsync(long? libraryId, long copyId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            long id = await ResolveLibraryIdAsync(client, libraryId);

            LibraryLoanReturn loanReturn = new()
            {
                ReturnedDate = PromptValue(nameof(LibraryLoanReturn.ReturnedDate), (DateOnly?)DateOnly.FromDateTime(DateTime.UtcNow)),
                Notes = PromptString(nameof(LibraryLoanReturn.Notes)),
            };

            LibraryLoan loan = await client.Library.ReturnCopyAsync(id, copyId, loanReturn);
            Console.WriteLine(loan.ToJsonString(indented: true));
        }

        private static async Task<LibraryCopy> PromptAndCreateCopyAsync(NsTcpWtfClient client, long libraryId, LibraryItem item, List<LibraryLocation> locations)
        {
            LibraryCopy copy = new() { ItemId = item.Id };
            if (locations.Count > 0)
            {
                WriteLocations(locations);
                copy.LocationId = PromptValue<long>($"{nameof(copy.LocationId)} (blank for none)");
            }
            PromptCopyFields(copy, item.MediaType);
            return await client.Library.CreateCopyAsync(libraryId, copy);
        }

        private static void PromptCopyFields(LibraryCopy copy, LibraryMediaType mediaType)
        {
            string formatHint = mediaType switch
            {
                LibraryMediaType.Book => " (e.g. Hardcover, Paperback, Mass market)",
                LibraryMediaType.Movie => " (e.g. DVD, Blu-ray, 4K UHD)",
                LibraryMediaType.BoardGame => " (e.g. Base game, Expansion)",
                _ => string.Empty,
            };
            copy.Format = PromptString($"{nameof(copy.Format)}{formatHint}", copy.Format);
            copy.Barcode = PromptString($"{nameof(copy.Barcode)} (ISBN, UPC or EAN, as printed)", copy.Barcode);
            if (mediaType == LibraryMediaType.Book)
            {
                copy.Isbn = PromptString($"{nameof(copy.Isbn)} (only if the barcode is a UPC; from the copyright page)", copy.Isbn == copy.Barcode ? null : copy.Isbn);
            }
            copy.Publisher = PromptString(nameof(copy.Publisher), copy.Publisher);
            copy.PublishedYear = PromptValue(nameof(copy.PublishedYear), copy.PublishedYear);
            copy.Condition = PromptString(nameof(copy.Condition), copy.Condition);
            copy.AcquiredDate = PromptValue($"{nameof(copy.AcquiredDate)} (blank if unknown)", copy.AcquiredDate);
            copy.AcquiredFrom = PromptString(nameof(copy.AcquiredFrom), copy.AcquiredFrom);
            copy.PricePaid = PromptValue(nameof(copy.PricePaid), copy.PricePaid);
            copy.Notes = PromptString(nameof(copy.Notes), copy.Notes);
        }
    }
}
