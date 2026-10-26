using System.CommandLine;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts.Libraries;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LibraryCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("library", "Track physical media: libraries, locations, items, copies and loans.");
            rootCommand.Aliases.Add("lib");

            #region Common

            Option<bool> rawOption = new("--raw") { Description = "Show raw json object(s) instead of the nicer summary format." };
            Option<long?> libraryOption = new("--library", "-L") { Description = "The id of the library. Can be left out when you can access only one." };
            Option<string> notesOption = new("--notes") { Description = "Notes to record on the event." };

            Argument<long> libraryIdArgument = new("libraryId") { Description = "The id of the library." };
            Argument<long> itemIdArgument = new("itemId") { Description = "The id of the item." };
            Argument<long> copyIdArgument = new("copyId") { Description = "The id of the copy." };
            Argument<long> locationIdArgument = new("locationId") { Description = "The id of the location." };

            #endregion // Common


            #region Library

            Command libraryCreateCommand = new("create", "Create a new library, owned by you.")
            {
                stampOption,
                verbosityOption,
            };
            libraryCreateCommand.AddCreateAliases();
            libraryCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleLibraryCreateAsync(stamp, verbosity);
                });

            Command libraryReadCommand = new("read", "Read an existing library.")
            {
                stampOption,
                verbosityOption,
                libraryIdArgument,
            };
            libraryReadCommand.AddReadAliases();
            libraryReadCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long libraryId = parseResult.GetRequiredValue(libraryIdArgument);
                    return HandleLibraryGetAsync(libraryId, stamp, verbosity);
                });

            Command libraryUpdateCommand = new("update", "Rename or redescribe a library you own.")
            {
                stampOption,
                verbosityOption,
                libraryIdArgument,
            };
            libraryUpdateCommand.AddUpdateAliases();
            libraryUpdateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long libraryId = parseResult.GetRequiredValue(libraryIdArgument);
                    return HandleLibraryEditAsync(libraryId, stamp, verbosity);
                });

            Command libraryDeleteCommand = new("delete", "Delete a library you own, with everything in it.")
            {
                stampOption,
                verbosityOption,
                libraryIdArgument,
            };
            libraryDeleteCommand.AddDeleteAliases();
            libraryDeleteCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long libraryId = parseResult.GetRequiredValue(libraryIdArgument);
                    return HandleLibraryDeleteAsync(libraryId, stamp, verbosity);
                });

            Command libraryListCommand = new("list", "List the libraries you own and the ones shared with you.")
            {
                stampOption,
                verbosityOption,
            };
            libraryListCommand.AddListAliases();
            libraryListCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleLibraryListAsync(stamp, verbosity);
                });

            rootCommand.Subcommands.Add(libraryCreateCommand);
            rootCommand.Subcommands.Add(libraryReadCommand);
            rootCommand.Subcommands.Add(libraryUpdateCommand);
            rootCommand.Subcommands.Add(libraryDeleteCommand);
            rootCommand.Subcommands.Add(libraryListCommand);

            #endregion // Library


            #region Share

            Command shareCommand = new("share", "Manage who a library is shared with.");
            shareCommand.Aliases.Add("sh");

            Argument<string> shareAccountNameArgument = new("accountName") { Description = "The name of the account." };

            Command shareSetCommand = new("set", "Share a library with an account, or change the permission of an existing share.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                shareAccountNameArgument,
                new Argument<LibraryPermission>("permission") { Description = $"{LibraryPermission.Viewer} or {LibraryPermission.Editor}." },
            };
            shareSetCommand.AddCreateAliases();
            shareSetCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    string accountName = parseResult.GetRequiredValue(shareAccountNameArgument);
                    LibraryPermission permission = parseResult.GetRequiredValue<LibraryPermission>("permission");
                    return HandleShareSetAsync(libraryId, accountName, permission, stamp, verbosity);
                });

            Command shareDeleteCommand = new("delete", "Remove an account's share. Removing your own leaves a library shared with you.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                shareAccountNameArgument,
            };
            shareDeleteCommand.AddDeleteAliases();
            shareDeleteCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    string accountName = parseResult.GetRequiredValue(shareAccountNameArgument);
                    return HandleShareDeleteAsync(libraryId, accountName, stamp, verbosity);
                });

            Command shareListCommand = new("list", "List the shares of a library you own.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
            };
            shareListCommand.AddListAliases();
            shareListCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    return HandleShareListAsync(libraryId, stamp, verbosity);
                });

            shareCommand.Subcommands.Add(shareSetCommand);
            shareCommand.Subcommands.Add(shareDeleteCommand);
            shareCommand.Subcommands.Add(shareListCommand);
            rootCommand.Subcommands.Add(shareCommand);

            #endregion // Share


            #region Location

            Command locationCommand = new("location", "Manage the places copies are kept, such as rooms and shelves.");
            locationCommand.Aliases.Add("loc");

            Command locationCreateCommand = new("create", "Create a new location, optionally inside another.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
            };
            locationCreateCommand.AddCreateAliases();
            locationCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    return HandleLocationCreateAsync(libraryId, stamp, verbosity);
                });

            Command locationUpdateCommand = new("update", "Rename, renote or move a location.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                locationIdArgument,
            };
            locationUpdateCommand.AddUpdateAliases();
            locationUpdateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    long locationId = parseResult.GetRequiredValue(locationIdArgument);
                    return HandleLocationEditAsync(libraryId, locationId, stamp, verbosity);
                });

            Command locationDeleteCommand = new("delete", "Delete an empty location.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                locationIdArgument,
            };
            locationDeleteCommand.AddDeleteAliases();
            locationDeleteCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    long locationId = parseResult.GetRequiredValue(locationIdArgument);
                    return HandleLocationDeleteAsync(libraryId, locationId, stamp, verbosity);
                });

            Command locationListCommand = new("list", "List the locations in a library.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
            };
            locationListCommand.AddListAliases();
            locationListCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    return HandleLocationListAsync(libraryId, stamp, verbosity);
                });

            locationCommand.Subcommands.Add(locationCreateCommand);
            locationCommand.Subcommands.Add(locationUpdateCommand);
            locationCommand.Subcommands.Add(locationDeleteCommand);
            locationCommand.Subcommands.Add(locationListCommand);
            rootCommand.Subcommands.Add(locationCommand);

            #endregion // Location


            #region Item

            Command itemCommand = new("item", "Manage items (a work, such as a book or a movie, of which you own one or more copies).");
            itemCommand.Aliases.Add("i");

            Command itemCreateCommand = new("create", "Create a new item, then optionally add its first copy.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
            };
            itemCreateCommand.AddCreateAliases();
            itemCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    return HandleItemCreateAsync(libraryId, stamp, verbosity);
                });

            Command itemReadCommand = new("read", "Read an item with its copies, loans and history.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                itemIdArgument,
                rawOption,
            };
            itemReadCommand.AddReadAliases();
            itemReadCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    long itemId = parseResult.GetRequiredValue(itemIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleItemGetAsync(libraryId, itemId, raw, stamp, verbosity);
                });

            Command itemUpdateCommand = new("update", "Update an item's descriptive fields.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                itemIdArgument,
            };
            itemUpdateCommand.AddUpdateAliases();
            itemUpdateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    long itemId = parseResult.GetRequiredValue(itemIdArgument);
                    return HandleItemEditAsync(libraryId, itemId, stamp, verbosity);
                });

            Command itemDeleteCommand = new("delete", "Delete an item with its copies, loans and history.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                itemIdArgument,
            };
            itemDeleteCommand.AddDeleteAliases();
            itemDeleteCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    long itemId = parseResult.GetRequiredValue(itemIdArgument);
                    return HandleItemDeleteAsync(libraryId, itemId, stamp, verbosity);
                });

            Argument<string> itemQueryArgument = new("query")
            {
                Description = "Text to match against title, subtitle, creators and series, or a barcode.",
                Arity = ArgumentArity.ZeroOrOne,
            };
            Option<LibraryMediaType?> itemMediaTypeOption = new("--type") { Description = "Only this media type." };
            Option<long?> itemLocationOption = new("--location") { Description = "Only items with a copy at this location or anywhere inside it." };
            Option<bool> itemLoanedOutOption = new("--loaned-out") { Description = "Only items with a copy on loan." };
            Option<int> skipOption = new("--skip") { Description = "How many results to skip.", DefaultValueFactory = _ => 0 };
            Option<int> takeOption = new("--take") { Description = "How many results to return, at most 500.", DefaultValueFactory = _ => 100 };
            Command itemListCommand = new("list", "Search or browse the items in a library.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                itemQueryArgument,
                itemMediaTypeOption,
                itemLocationOption,
                itemLoanedOutOption,
                skipOption,
                takeOption,
                rawOption,
            };
            itemListCommand.AddListAliases();
            itemListCommand.Aliases.Add("search");
            itemListCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    string query = parseResult.GetValue(itemQueryArgument);
                    LibraryMediaType? mediaType = parseResult.GetValue(itemMediaTypeOption);
                    long? locationId = parseResult.GetValue(itemLocationOption);
                    bool loanedOut = parseResult.GetValue(itemLoanedOutOption);
                    int skip = parseResult.GetRequiredValue(skipOption);
                    int take = parseResult.GetRequiredValue(takeOption);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleItemListAsync(libraryId, query, mediaType, locationId, loanedOut, skip, take, raw, stamp, verbosity);
                });

            itemCommand.Subcommands.Add(itemCreateCommand);
            itemCommand.Subcommands.Add(itemReadCommand);
            itemCommand.Subcommands.Add(itemUpdateCommand);
            itemCommand.Subcommands.Add(itemDeleteCommand);
            itemCommand.Subcommands.Add(itemListCommand);
            rootCommand.Subcommands.Add(itemCommand);

            #endregion // Item


            #region Copy

            Command copyCommand = new("copy", "Manage copies (one physical object: a format, a barcode, a location and loans).");
            copyCommand.Aliases.Add("cp");

            Command copyCreateCommand = new("create", "Add a copy of an existing item.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                itemIdArgument,
            };
            copyCreateCommand.AddCreateAliases();
            copyCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    long itemId = parseResult.GetRequiredValue(itemIdArgument);
                    return HandleCopyCreateAsync(libraryId, itemId, stamp, verbosity);
                });

            Command copyReadCommand = new("read", "Read an existing copy.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                copyIdArgument,
            };
            copyReadCommand.AddReadAliases();
            copyReadCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    long copyId = parseResult.GetRequiredValue(copyIdArgument);
                    return HandleCopyGetAsync(libraryId, copyId, stamp, verbosity);
                });

            Command copyUpdateCommand = new("update", "Update a copy's descriptive fields, or move it to another item to merge a duplicate.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                copyIdArgument,
            };
            copyUpdateCommand.AddUpdateAliases();
            copyUpdateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    long copyId = parseResult.GetRequiredValue(copyIdArgument);
                    return HandleCopyEditAsync(libraryId, copyId, stamp, verbosity);
                });

            Command copyDeleteCommand = new("delete", "Delete a copy with its loans and history. To keep the history of a copy that's gone, dispose it instead.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                copyIdArgument,
            };
            copyDeleteCommand.AddDeleteAliases();
            copyDeleteCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    long copyId = parseResult.GetRequiredValue(copyIdArgument);
                    return HandleCopyDeleteAsync(libraryId, copyId, stamp, verbosity);
                });

            Command copyMoveCommand = new("move", "Move a copy to another location, or to none.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                copyIdArgument,
                notesOption,
            };
            copyMoveCommand.Aliases.Add("mv");
            copyMoveCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    long copyId = parseResult.GetRequiredValue(copyIdArgument);
                    string notes = parseResult.GetValue(notesOption);
                    return HandleCopyMoveAsync(libraryId, copyId, notes, stamp, verbosity);
                });

            Command copyStatusCommand = new("status", "Mark a copy missing or disposed, or restore it to active.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                copyIdArgument,
                new Argument<LibraryCopyStatus>("status") { Description = $"{LibraryCopyStatus.Active}, {LibraryCopyStatus.Missing} or {LibraryCopyStatus.Disposed}." },
                notesOption,
            };
            copyStatusCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    long copyId = parseResult.GetRequiredValue(copyIdArgument);
                    LibraryCopyStatus status = parseResult.GetRequiredValue<LibraryCopyStatus>("status");
                    string notes = parseResult.GetValue(notesOption);
                    return HandleCopyStatusAsync(libraryId, copyId, status, notes, stamp, verbosity);
                });

            Command copyLoanCommand = new("loan", "Lend a copy.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                copyIdArgument,
            };
            copyLoanCommand.Aliases.Add("lend");
            copyLoanCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    long copyId = parseResult.GetRequiredValue(copyIdArgument);
                    return HandleCopyLoanAsync(libraryId, copyId, stamp, verbosity);
                });

            Command copyReturnCommand = new("return", "Record a lent copy coming back.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                copyIdArgument,
            };
            copyReturnCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    long copyId = parseResult.GetRequiredValue(copyIdArgument);
                    return HandleCopyReturnAsync(libraryId, copyId, stamp, verbosity);
                });

            copyCommand.Subcommands.Add(copyCreateCommand);
            copyCommand.Subcommands.Add(copyReadCommand);
            copyCommand.Subcommands.Add(copyUpdateCommand);
            copyCommand.Subcommands.Add(copyDeleteCommand);
            copyCommand.Subcommands.Add(copyMoveCommand);
            copyCommand.Subcommands.Add(copyStatusCommand);
            copyCommand.Subcommands.Add(copyLoanCommand);
            copyCommand.Subcommands.Add(copyReturnCommand);
            rootCommand.Subcommands.Add(copyCommand);

            #endregion // Copy


            #region Loan

            Command loanCommand = new("loan", "See what's lent out.");
            loanCommand.Aliases.Add("ln");

            Option<bool> loanAllOption = new("--all") { Description = "Include returned loans, newest first." };
            Command loanListCommand = new("list", "List open loans, by due date.")
            {
                stampOption,
                verbosityOption,
                libraryOption,
                loanAllOption,
                skipOption,
                takeOption,
            };
            loanListCommand.AddListAliases();
            loanListCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? libraryId = parseResult.GetValue(libraryOption);
                    bool all = parseResult.GetValue(loanAllOption);
                    int skip = parseResult.GetRequiredValue(skipOption);
                    int take = parseResult.GetRequiredValue(takeOption);
                    return HandleLoanListAsync(libraryId, all, skip, take, stamp, verbosity);
                });

            loanCommand.Subcommands.Add(loanListCommand);
            rootCommand.Subcommands.Add(loanCommand);

            #endregion // Loan


            #region Lookup

            Option<LibraryMediaType> lookupTypeOption = new("--type")
            {
                Description = "What to search for by title. Not needed for an ISBN.",
                DefaultValueFactory = _ => LibraryMediaType.Book,
            };
            Option<string> lookupSourceOption = new("--source")
            {
                Description = $"Get one record by id from this source instead of searching: {LibraryLookupSources.OpenLibrary}, {LibraryLookupSources.TmdbMovie}, {LibraryLookupSources.TmdbTv} or {LibraryLookupSources.BoardGameGeek}. The source and id are the first two columns of a search.",
            };
            Command lookupCommand = new("lookup", "Look up a book by ISBN, or search by title, in the outside services the add page uses. Saves nothing. Writes a table with fixed columns, or json with --raw, and nothing else.")
            {
                stampOption,
                verbosityOption,
                new Argument<string>("query") { Description = "An ISBN, a title, or with --source the record's id." },
                lookupTypeOption,
                lookupSourceOption,
                rawOption,
            };
            lookupCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string query = parseResult.GetRequiredValue<string>("query");
                    LibraryMediaType mediaType = parseResult.GetRequiredValue(lookupTypeOption);
                    string source = parseResult.GetValue(lookupSourceOption);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleLookupAsync(query, mediaType, source, raw, stamp, verbosity);
                });
            rootCommand.Subcommands.Add(lookupCommand);

            #endregion // Lookup


            #region Owned

            Command ownedCommand = new("owned", "Do I already own this? Looks a barcode up across every library you can access.")
            {
                stampOption,
                verbosityOption,
                new Argument<string>("barcode") { Description = "The barcode: ISBN-10 or -13, UPC or EAN. Dashes and spaces are fine." },
                rawOption,
            };
            ownedCommand.Aliases.Add("own");
            ownedCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    string barcode = parseResult.GetRequiredValue<string>("barcode");
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleOwnedAsync(barcode, raw, stamp, verbosity);
                });
            rootCommand.Subcommands.Add(ownedCommand);

            #endregion // Owned


            return rootCommand;
        }
    }
}
