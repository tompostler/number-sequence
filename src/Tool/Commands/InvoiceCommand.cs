using System.CommandLine;
using TcpWtf.NumberSequence.Client;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class InvoiceCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("invoice", "Create and manage invoices.");

            // Common arguments
            Argument<long> idArgument = new("invoiceId") { Description = "The id of the invoice." };
            Option<bool> rawOption = new("--raw") { Description = "Show raw json object(s) instead of the nicer summary format." };

            Command businessCommand = new("business", "Manage invoice invoices (location that creates invoices).");

            Argument<long> businessIdArgument = new("businessId") { Description = "The id of the business." };

            Command businessCreateCommand = new("create", "Create a new invoice business.")
            {
                stampOption,
                verbosityOption,
            };
            businessCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleBusinessCreateAsync(stamp, verbosity);
                });

            Command businessGetCommand = new("get", "Get an existing invoice business.")
            {
                stampOption,
                verbosityOption,
                businessIdArgument,
            };
            businessGetCommand.Aliases.Add("show");
            businessGetCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long businessId = parseResult.GetRequiredValue(businessIdArgument);
                    return HandleBusinessGetAsync(businessId, stamp, verbosity);
                });

            Command businessListCommand = new("list", "Get existing invoice businesses.")
            {
                stampOption,
                verbosityOption,
            };
            businessListCommand.Aliases.Add("ls");
            businessListCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleBusinessListAsync(stamp, verbosity);
                });

            businessCommand.Subcommands.Add(businessCreateCommand);
            businessCommand.Subcommands.Add(businessGetCommand);
            businessCommand.Subcommands.Add(businessListCommand);
            rootCommand.Subcommands.Add(businessCommand);


            Command customerCommand = new("customer", "Manage invoice customers (location that receives invoices).");

            Command customerCreateCommand = new("create", "Create a new invoice customer.")
            {
                stampOption,
                verbosityOption,
            };
            customerCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleCustomerCreateAsync(stamp, verbosity);
                });

            Argument<long> customerIdArgument = new("customerId") { Description = "The id of the customer." };
            Command customerEditCommand = new("edit", "Edit an existing invoice customer.")
            {
                stampOption,
                verbosityOption,
                customerIdArgument,
            };
            customerEditCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long customerId = parseResult.GetRequiredValue(customerIdArgument);
                    return HandleCustomerEditAsync(customerId, stamp, verbosity);
                });

            Command customerGetCommand = new("get", "Get an existing invoice customer.")
            {
                stampOption,
                verbosityOption,
                customerIdArgument,
            };
            customerGetCommand.Aliases.Add("show");
            customerGetCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long customerId = parseResult.GetRequiredValue(customerIdArgument);
                    return HandleCustomerGetAsync(customerId, stamp, verbosity);
                });

            Command customerListCommand = new("list", "Get existing invoice customers.")
            {
                stampOption,
                verbosityOption,
            };
            customerListCommand.Aliases.Add("ls");
            customerListCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleCustomerListAsync(stamp, verbosity);
                });

            customerCommand.Subcommands.Add(customerCreateCommand);
            customerCommand.Subcommands.Add(customerEditCommand);
            customerCommand.Subcommands.Add(customerGetCommand);
            customerCommand.Subcommands.Add(customerListCommand);
            rootCommand.Subcommands.Add(customerCommand);


            Command lineDefaultCommand = new("line-default", "Manage invoice line defaults (line items that can be used as a reference for adding a line to an invoice).");

            Command lineDefaultCreateCommand = new("create", "Create a new invoice line default.")
            {
                stampOption,
                verbosityOption,
            };
            lineDefaultCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleLineDefaultCreateAsync(stamp, verbosity);
                });

            Argument<long> lineDefaultIdArgument = new("lineDefaultId") { Description = "The id of the line default." };
            Command lineDefaultEditCommand = new("edit", "Edit an existing invoice line.")
            {
                stampOption,
                verbosityOption,
                lineDefaultIdArgument,
            };
            lineDefaultEditCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long id = parseResult.GetRequiredValue(lineDefaultIdArgument);
                    return HandleLineDefaultEditAsync(id, stamp, verbosity);
                });

            Command lineDefaultGetCommand = new("get", "Get an existing invoice line default.")
            {
                stampOption,
                verbosityOption,
                lineDefaultIdArgument,
            };
            lineDefaultGetCommand.Aliases.Add("show");
            lineDefaultGetCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long id = parseResult.GetRequiredValue(lineDefaultIdArgument);
                    return HandleLineDefaultGetAsync(id, stamp, verbosity);
                });

            Command lineDefaultListCommand = new("list", "Get existing invoice line defaults.")
            {
                stampOption,
                verbosityOption,
            };
            lineDefaultListCommand.Aliases.Add("ls");
            lineDefaultListCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleLineDefaultListAsync(stamp, verbosity);
                });

            Command lineDefaultSwapCommand = new("swap", "Since line defaults are a CLI utility, swap two of them for more preferred sorting.")
            {
                stampOption,
                verbosityOption,
                lineDefaultIdArgument,
                new Argument<long>("lineDefaultOtherId") { Description = "The id of the other line default." },
            };
            lineDefaultSwapCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long id = parseResult.GetRequiredValue(lineDefaultIdArgument);
                    long otherId = parseResult.GetRequiredValue<long>("lineDefaultOtherId");
                    return HandleLineDefaultSwapAsync(id, otherId, stamp, verbosity);
                });

            lineDefaultCommand.Subcommands.Add(lineDefaultCreateCommand);
            lineDefaultCommand.Subcommands.Add(lineDefaultEditCommand);
            lineDefaultCommand.Subcommands.Add(lineDefaultGetCommand);
            lineDefaultCommand.Subcommands.Add(lineDefaultListCommand);
            lineDefaultCommand.Subcommands.Add(lineDefaultSwapCommand);
            rootCommand.Subcommands.Add(lineDefaultCommand);


            Command lineCommand = new("line", "Manage invoice lines.");

            Argument<long> lineIdArgument = new("lineId") { Description = "The id of the line." };
            Argument<long> lineOtherIdArgument = new("lineOtherId") { Description = "The id of the other line." };
            Argument<long> targetInvoiceIdArgument = new("target-invoice-id") { Description = "If supplied, the target invoice id to create the duplicate line on.", DefaultValueFactory = _ => -1L };

            Command lineCreateCommand = new("create", "Create a new invoice line.")
            {
                stampOption,
                verbosityOption,
                idArgument,
                rawOption,
            };
            lineCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(idArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleLineCreateAsync(invoiceId, raw, stamp, verbosity);
                });

            Command lineDuplicateCommand = new("duplicate", "Duplicate an invoice line, optionally to another invoice.")
            {
                stampOption,
                verbosityOption,
                idArgument,
                lineIdArgument,
                targetInvoiceIdArgument,
                rawOption,
            };
            lineDuplicateCommand.Aliases.Add("dupe");
            lineDuplicateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(idArgument);
                    long lineId = parseResult.GetRequiredValue(lineIdArgument);
                    long targetId = parseResult.GetRequiredValue(targetInvoiceIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleLineDuplicateAsync(invoiceId, lineId, targetId, raw, stamp, verbosity);
                });

            Command lineEditCommand = new("edit", "Edit an existing invoice line.")
            {
                stampOption,
                verbosityOption,
                idArgument,
                lineIdArgument,
                rawOption,
            };
            lineEditCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(idArgument);
                    long lineId = parseResult.GetRequiredValue(lineIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleLineEditAsync(invoiceId, lineId, raw, stamp, verbosity);
                });

            Command lineGetCommand = new("get", "Get an existing invoice line.")
            {
                stampOption,
                verbosityOption,
                idArgument,
                lineIdArgument,
            };
            lineGetCommand.Aliases.Add("show");
            lineGetCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(idArgument);
                    long lineId = parseResult.GetRequiredValue(lineIdArgument);
                    return HandleLineGetAsync(invoiceId, lineId, stamp, verbosity);
                });

            Command lineListCommand = new("list", "Get existing invoice lines.")
            {
                stampOption,
                verbosityOption,
                idArgument,
            };
            lineListCommand.Aliases.Add("ls");
            lineListCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(idArgument);
                    return HandleLineListAsync(invoiceId, stamp, verbosity);
                });

            Command lineRemoveCommand = new("remove", "Remove an existing invoice line.")
            {
                stampOption,
                verbosityOption,
                idArgument,
                lineIdArgument,
                rawOption,
            };
            lineRemoveCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(idArgument);
                    long lineId = parseResult.GetRequiredValue(lineIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleLineRemoveAsync(invoiceId, lineId, raw, stamp, verbosity);
                });

            Command lineSwapCommand = new("swap", "Swap lines in an invoice to change their ordering to a more preferred sorting.")
            {
                stampOption,
                verbosityOption,
                idArgument,
                lineIdArgument,
                lineOtherIdArgument,
                rawOption,
            };
            lineSwapCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(idArgument);
                    long lineId = parseResult.GetRequiredValue(lineIdArgument);
                    long otherId = parseResult.GetRequiredValue(lineOtherIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleLineSwapAsync(invoiceId, lineId, otherId, raw, stamp, verbosity);
                });

            lineCommand.Subcommands.Add(lineCreateCommand);
            lineCommand.Subcommands.Add(lineDuplicateCommand);
            lineCommand.Subcommands.Add(lineEditCommand);
            lineCommand.Subcommands.Add(lineGetCommand);
            lineCommand.Subcommands.Add(lineListCommand);
            lineCommand.Subcommands.Add(lineRemoveCommand);
            lineCommand.Subcommands.Add(lineSwapCommand);
            rootCommand.Subcommands.Add(lineCommand);


            Command createCommand = new("create", "Create a new invoice.")
            {
                stampOption,
                verbosityOption,
                new Option<long?>("--from") { Description = "The id of the invoice if creating from another invoice. The first two lines of the new invoice will be the total of the previous and the payment information." },
                rawOption,
            };
            createCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? from = parseResult.GetValue(new Option<long?>("--from"));
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleCreateAsync(from, raw, stamp, verbosity);
                });
            rootCommand.Subcommands.Add(createCommand);


            Command editCommand = new("edit", "Edit an existing invoice.")
            {
                stampOption,
                verbosityOption,
                idArgument,
                rawOption,
            };
            editCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(idArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleEditAsync(invoiceId, raw, stamp, verbosity);
                });
            rootCommand.Subcommands.Add(editCommand);


            Command getCommand = new("get", "Get an existing invoice.")
            {
                stampOption,
                verbosityOption,
                idArgument,
                rawOption,
            };
            getCommand.Aliases.Add("show");
            getCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(idArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleGetAsync(invoiceId, raw, stamp, verbosity);
                });
            rootCommand.Subcommands.Add(getCommand);


            Command listCommand = new("list", "Get existing invoices.")
            {
                stampOption,
                verbosityOption,
            };
            listCommand.Aliases.Add("ls");
            listCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleListAsync(stamp, verbosity);
                });
            rootCommand.Subcommands.Add(listCommand);


            Command markPaidCommand = new("mark-paid", "Mark a specific invoice as paid.")
            {
                stampOption,
                verbosityOption,
                idArgument,
                rawOption,
            };
            markPaidCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(idArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleMarkPaidAsync(invoiceId, raw, stamp, verbosity);
                });
            rootCommand.Subcommands.Add(markPaidCommand);


            Command markReprocessRegularlyCommand = new("mark-reprocess-regularly", "Mark a specific invoice to be automatically reprocessed to pdf every 14d if not marked as paid.")
            {
                stampOption,
                verbosityOption,
                idArgument,
                rawOption,
            };
            markReprocessRegularlyCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(idArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleMarkReprocessRegularlyAsync(invoiceId, raw, stamp, verbosity);
                });
            rootCommand.Subcommands.Add(markReprocessRegularlyCommand);


            Command processCommand = new("process", "Mark a specific invoice for [re-]processing to pdf.")
            {
                stampOption,
                verbosityOption,
                idArgument,
                rawOption,
            };
            processCommand.Aliases.Add("reprocess");
            processCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(idArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleProcessAsync(invoiceId, raw, stamp, verbosity);
                });
            rootCommand.Subcommands.Add(processCommand);


            return rootCommand;
        }
    }
}
