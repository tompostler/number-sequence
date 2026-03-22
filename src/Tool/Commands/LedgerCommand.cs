using System.CommandLine;
using TcpWtf.NumberSequence.Client;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LedgerCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("ledger", "Create and manage ledger data.");
            rootCommand.Aliases.Add("l");

            #region Common

            // Common arguments shared across invoice and invoice line subcommands
            Option<bool> rawOption = new("--raw") { Description = "Show raw json object(s) instead of the nicer summary format." };

            #endregion // Common


            #region Business

            Command businessCommand = new("business", "Manage businesses (the entity that creates invoices).");
            businessCommand.Aliases.Add("b");

            Argument<long> businessIdArgument = new("businessId") { Description = "The id of the business." };

            Command businessCreateCommand = new("create", "Create a new business.")
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

            Command businessGetCommand = new("get", "Get an existing business.")
            {
                stampOption,
                verbosityOption,
                businessIdArgument,
            };
            businessGetCommand.Aliases.Add("show");
            businessGetCommand.Aliases.Add("read");
            businessGetCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long businessId = parseResult.GetRequiredValue(businessIdArgument);
                    return HandleBusinessGetAsync(businessId, stamp, verbosity);
                });

            Command businessListCommand = new("list", "Get existing businesses.")
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

            Command businessLogoCommand = new("logo", "Manage the logo image for a business.");

            Argument<FileInfo> logoFileInfoArgument = new Argument<FileInfo>("file-path") { Description = "Path to the image file (.gif, .jpg, .jpeg, .png, .webp). Maximum 64kb." }.AcceptExistingOnly();
            Command businessLogoCreateCommand = new("create", "Upload a logo for an existing business.")
            {
                stampOption,
                verbosityOption,
                businessIdArgument,
                logoFileInfoArgument,
            };
            businessLogoCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long businessId = parseResult.GetRequiredValue(businessIdArgument);
                    FileInfo logoFileInfo = parseResult.GetRequiredValue(logoFileInfoArgument);
                    return HandleBusinessLogoCreateAsync(businessId, logoFileInfo, stamp, verbosity);
                });

            Command businessLogoUpdateCommand = new("update", "Replace the logo for an existing business.")
            {
                stampOption,
                verbosityOption,
                businessIdArgument,
                logoFileInfoArgument,
            };
            businessLogoUpdateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long businessId = parseResult.GetRequiredValue(businessIdArgument);
                    FileInfo logoFileInfo = parseResult.GetRequiredValue(logoFileInfoArgument);
                    return HandleBusinessLogoUpdateAsync(businessId, logoFileInfo, stamp, verbosity);
                });

            Argument<FileInfo> outputFileInfoArgument = new Argument<FileInfo>("output-path") { Description = "Path to write the downloaded image file." }.AcceptLegalFilePathsOnly();
            Command businessLogoGetCommand = new("get", "Download the logo for an existing business.")
            {
                stampOption,
                verbosityOption,
                businessIdArgument,
                outputFileInfoArgument,
            };
            businessLogoGetCommand.Aliases.Add("show");
            businessLogoGetCommand.Aliases.Add("read");
            businessLogoGetCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long businessId = parseResult.GetRequiredValue(businessIdArgument);
                    FileInfo outputFileInfo = parseResult.GetRequiredValue(outputFileInfoArgument);
                    return HandleBusinessLogoGetAsync(businessId, outputFileInfo, stamp, verbosity);
                });

            Command businessLogoDeleteCommand = new("delete", "Delete the logo for an existing business.")
            {
                stampOption,
                verbosityOption,
                businessIdArgument,
            };
            businessLogoDeleteCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long businessId = parseResult.GetRequiredValue(businessIdArgument);
                    return HandleBusinessLogoDeleteAsync(businessId, stamp, verbosity);
                });

            businessLogoCommand.Subcommands.Add(businessLogoCreateCommand);
            businessLogoCommand.Subcommands.Add(businessLogoUpdateCommand);
            businessLogoCommand.Subcommands.Add(businessLogoGetCommand);
            businessLogoCommand.Subcommands.Add(businessLogoDeleteCommand);
            businessCommand.Subcommands.Add(businessLogoCommand);

            businessCommand.Subcommands.Add(businessCreateCommand);
            businessCommand.Subcommands.Add(businessGetCommand);
            businessCommand.Subcommands.Add(businessListCommand);
            rootCommand.Subcommands.Add(businessCommand);

            #endregion // Business


            #region Customer

            Command customerCommand = new("customer", "Manage customers (the entity that receives invoices).");
            customerCommand.Aliases.Add("c");

            Argument<long> customerIdArgument = new("customerId") { Description = "The id of the customer." };

            Command customerCreateCommand = new("create", "Create a new customer.")
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

            Command customerEditCommand = new("edit", "Edit an existing customer.")
            {
                stampOption,
                verbosityOption,
                customerIdArgument,
            };
            customerEditCommand.Aliases.Add("update");
            customerEditCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long customerId = parseResult.GetRequiredValue(customerIdArgument);
                    return HandleCustomerEditAsync(customerId, stamp, verbosity);
                });

            Command customerGetCommand = new("get", "Get an existing customer.")
            {
                stampOption,
                verbosityOption,
                customerIdArgument,
            };
            customerGetCommand.Aliases.Add("show");
            customerGetCommand.Aliases.Add("read");
            customerGetCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long customerId = parseResult.GetRequiredValue(customerIdArgument);
                    return HandleCustomerGetAsync(customerId, stamp, verbosity);
                });

            Command customerListCommand = new("list", "Get existing customers.")
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

            #endregion // Customer


            #region Statement

            Command statementCommand = new("statement", "Manage statements (a collection of invoices for a customer over a date range).");
            statementCommand.Aliases.Add("s");

            Argument<long> statementIdArgument = new("statementId") { Description = "The id of the statement." };

            Command statementCreateCommand = new("create", "Create a new statement.")
            {
                stampOption,
                verbosityOption,
            };
            statementCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleStatementCreateAsync(stamp, verbosity);
                });

            Command statementGetCommand = new("get", "Get an existing statement.")
            {
                stampOption,
                verbosityOption,
                statementIdArgument,
            };
            statementGetCommand.Aliases.Add("show");
            statementGetCommand.Aliases.Add("read");
            statementGetCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long statementId = parseResult.GetRequiredValue(statementIdArgument);
                    return HandleStatementGetAsync(statementId, stamp, verbosity);
                });

            Command statementListCommand = new("list", "Get existing statements.")
            {
                stampOption,
                verbosityOption,
            };
            statementListCommand.Aliases.Add("ls");
            statementListCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleStatementListAsync(stamp, verbosity);
                });

            Command statementProcessCommand = new("process", "Mark a specific statement for [re-]processing to pdf.")
            {
                stampOption,
                verbosityOption,
                statementIdArgument,
            };
            statementProcessCommand.Aliases.Add("reprocess");
            statementProcessCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long statementId = parseResult.GetRequiredValue(statementIdArgument);
                    return HandleStatementProcessAsync(statementId, stamp, verbosity);
                });

            statementCommand.Subcommands.Add(statementCreateCommand);
            statementCommand.Subcommands.Add(statementGetCommand);
            statementCommand.Subcommands.Add(statementListCommand);
            statementCommand.Subcommands.Add(statementProcessCommand);
            rootCommand.Subcommands.Add(statementCommand);

            #endregion // Statement


            #region Invoice

            Command invoiceCommand = new("invoice", "Create and manage invoices.");
            invoiceCommand.Aliases.Add("i");
            Argument<long> invoiceIdArgument = new("invoiceId") { Description = "The id of the invoice." };

            #region Invoice Lines

            Command invoiceLineCommand = new("line", "Manage invoice lines.");
            invoiceLineCommand.Aliases.Add("l");

            Argument<long> invoiceLineIdArgument = new("lineId") { Description = "The id of the line." };
            Argument<long> invoiceLineOtherIdArgument = new("lineOtherId") { Description = "The id of the other line." };
            Argument<long> targetInvoiceIdArgument = new("target-invoice-id") { Description = "If supplied, the target invoice id to create the duplicate line on.", DefaultValueFactory = _ => -1L };

            Command invoiceLineCreateCommand = new("create", "Create a new invoice line.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                rawOption,
            };
            invoiceLineCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleLineCreateAsync(invoiceId, raw, stamp, verbosity);
                });

            Command invoiceLineDuplicateCommand = new("duplicate", "Duplicate an invoice line, optionally to another invoice.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                invoiceLineIdArgument,
                targetInvoiceIdArgument,
                rawOption,
            };
            invoiceLineDuplicateCommand.Aliases.Add("dupe");
            invoiceLineDuplicateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    long lineId = parseResult.GetRequiredValue(invoiceLineIdArgument);
                    long targetId = parseResult.GetRequiredValue(targetInvoiceIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleLineDuplicateAsync(invoiceId, lineId, targetId, raw, stamp, verbosity);
                });

            Command invoiceLineEditCommand = new("edit", "Edit an existing invoice line.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                invoiceLineIdArgument,
                rawOption,
            };
            invoiceLineEditCommand.Aliases.Add("update");
            invoiceLineEditCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    long lineId = parseResult.GetRequiredValue(invoiceLineIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleLineEditAsync(invoiceId, lineId, raw, stamp, verbosity);
                });

            Command invoiceLineGetCommand = new("get", "Get an existing invoice line.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                invoiceLineIdArgument,
            };
            invoiceLineGetCommand.Aliases.Add("show");
            invoiceLineGetCommand.Aliases.Add("read");
            invoiceLineGetCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    long lineId = parseResult.GetRequiredValue(invoiceLineIdArgument);
                    return HandleLineGetAsync(invoiceId, lineId, stamp, verbosity);
                });

            Command invoiceLineListCommand = new("list", "Get existing invoice lines.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
            };
            invoiceLineListCommand.Aliases.Add("ls");
            invoiceLineListCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    return HandleLineListAsync(invoiceId, stamp, verbosity);
                });

            Command invoiceLineRemoveCommand = new("remove", "Remove an existing invoice line.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                invoiceLineIdArgument,
                rawOption,
            };
            invoiceLineRemoveCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    long lineId = parseResult.GetRequiredValue(invoiceLineIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleLineRemoveAsync(invoiceId, lineId, raw, stamp, verbosity);
                });

            Command invoiceLineSwapCommand = new("swap", "Swap lines in an invoice to change their ordering to a more preferred sorting.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                invoiceLineIdArgument,
                invoiceLineOtherIdArgument,
                rawOption,
            };
            invoiceLineSwapCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    long lineId = parseResult.GetRequiredValue(invoiceLineIdArgument);
                    long otherId = parseResult.GetRequiredValue(invoiceLineOtherIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleLineSwapAsync(invoiceId, lineId, otherId, raw, stamp, verbosity);
                });

            invoiceLineCommand.Subcommands.Add(invoiceLineCreateCommand);
            invoiceLineCommand.Subcommands.Add(invoiceLineDuplicateCommand);
            invoiceLineCommand.Subcommands.Add(invoiceLineEditCommand);
            invoiceLineCommand.Subcommands.Add(invoiceLineGetCommand);
            invoiceLineCommand.Subcommands.Add(invoiceLineListCommand);
            invoiceLineCommand.Subcommands.Add(invoiceLineRemoveCommand);
            invoiceLineCommand.Subcommands.Add(invoiceLineSwapCommand);
            invoiceCommand.Subcommands.Add(invoiceLineCommand);

            #endregion // Invoice Lines

            #region Invoice Line Defaults

            Command invoiceLineDefaultCommand = new("line-default", "Manage invoice line defaults (line items that can be used as a reference for adding a line to an invoice).");
            invoiceLineDefaultCommand.Aliases.Add("ld");

            Argument<long> invoiceLineDefaultIdArgument = new("lineDefaultId") { Description = "The id of the line default." };

            Command invoiceLineDefaultCreateCommand = new("create", "Create a new invoice line default.")
            {
                stampOption,
                verbosityOption,
            };
            invoiceLineDefaultCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleLineDefaultCreateAsync(stamp, verbosity);
                });

            Command invoiceLineDefaultEditCommand = new("edit", "Edit an existing invoice line default.")
            {
                stampOption,
                verbosityOption,
                invoiceLineDefaultIdArgument,
            };
            invoiceLineDefaultEditCommand.Aliases.Add("update");
            invoiceLineDefaultEditCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long id = parseResult.GetRequiredValue(invoiceLineDefaultIdArgument);
                    return HandleLineDefaultEditAsync(id, stamp, verbosity);
                });

            Command invoiceLineDefaultGetCommand = new("get", "Get an existing invoice line default.")
            {
                stampOption,
                verbosityOption,
                invoiceLineDefaultIdArgument,
            };
            invoiceLineDefaultGetCommand.Aliases.Add("show");
            invoiceLineDefaultGetCommand.Aliases.Add("read");
            invoiceLineDefaultGetCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long id = parseResult.GetRequiredValue(invoiceLineDefaultIdArgument);
                    return HandleLineDefaultGetAsync(id, stamp, verbosity);
                });

            Command invoiceLineDefaultListCommand = new("list", "Get existing invoice line defaults.")
            {
                stampOption,
                verbosityOption,
            };
            invoiceLineDefaultListCommand.Aliases.Add("ls");
            invoiceLineDefaultListCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleLineDefaultListAsync(stamp, verbosity);
                });

            Command invoiceLineDefaultSwapCommand = new("swap", "Since line defaults are a CLI utility, swap two of them for more preferred sorting.")
            {
                stampOption,
                verbosityOption,
                invoiceLineDefaultIdArgument,
                new Argument<long>("lineDefaultOtherId") { Description = "The id of the other line default." },
            };
            invoiceLineDefaultSwapCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long id = parseResult.GetRequiredValue(invoiceLineDefaultIdArgument);
                    long otherId = parseResult.GetRequiredValue<long>("lineDefaultOtherId");
                    return HandleLineDefaultSwapAsync(id, otherId, stamp, verbosity);
                });

            invoiceLineDefaultCommand.Subcommands.Add(invoiceLineDefaultCreateCommand);
            invoiceLineDefaultCommand.Subcommands.Add(invoiceLineDefaultEditCommand);
            invoiceLineDefaultCommand.Subcommands.Add(invoiceLineDefaultGetCommand);
            invoiceLineDefaultCommand.Subcommands.Add(invoiceLineDefaultListCommand);
            invoiceLineDefaultCommand.Subcommands.Add(invoiceLineDefaultSwapCommand);
            invoiceCommand.Subcommands.Add(invoiceLineDefaultCommand);

            #endregion // Invoice Line Defaults

            Option<long?> idFromOption = new("--from") { Description = "The id of the invoice if creating from another invoice. The first two lines of the new invoice will be the total of the previous and the payment information." };
            Command createCommand = new("create", "Create a new invoice.")
            {
                stampOption,
                verbosityOption,
                idFromOption,
                rawOption,
            };
            createCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long? from = parseResult.GetValue(idFromOption);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleCreateAsync(from, raw, stamp, verbosity);
                });
            invoiceCommand.Subcommands.Add(createCommand);

            Command invoiceEditCommand = new("edit", "Edit an existing invoice.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                rawOption,
            };
            invoiceEditCommand.Aliases.Add("update");
            invoiceEditCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleEditAsync(invoiceId, raw, stamp, verbosity);
                });
            invoiceCommand.Subcommands.Add(invoiceEditCommand);

            Command invoiceGetCommand = new("get", "Get an existing invoice.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                rawOption,
            };
            invoiceGetCommand.Aliases.Add("show");
            invoiceGetCommand.Aliases.Add("read");
            invoiceGetCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleGetAsync(invoiceId, raw, stamp, verbosity);
                });
            invoiceCommand.Subcommands.Add(invoiceGetCommand);

            Command invoiceListCommand = new("list", "Get existing invoices.")
            {
                stampOption,
                verbosityOption,
            };
            invoiceListCommand.Aliases.Add("ls");
            invoiceListCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleListAsync(stamp, verbosity);
                });
            invoiceCommand.Subcommands.Add(invoiceListCommand);

            #region Invoice Payments

            Command invoicePaymentCommand = new("payment", "Manage payments on an invoice.");
            invoicePaymentCommand.Aliases.Add("p");

            Argument<long> invoicePaymentIdArgument = new("paymentId") { Description = "The id of the payment." };

            Command invoicePaymentAddCommand = new("add", "Add a payment to an existing invoice.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                rawOption,
            };
            invoicePaymentAddCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandlePaymentAddAsync(invoiceId, raw, stamp, verbosity);
                });

            Command invoicePaymentEditCommand = new("edit", "Edit the date or details of an existing payment.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                invoicePaymentIdArgument,
                rawOption,
            };
            invoicePaymentEditCommand.Aliases.Add("update");
            invoicePaymentEditCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    long paymentId = parseResult.GetRequiredValue(invoicePaymentIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandlePaymentEditAsync(invoiceId, paymentId, raw, stamp, verbosity);
                });

            Command invoicePaymentRemoveCommand = new("remove", "Remove a payment from an existing invoice.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                invoicePaymentIdArgument,
                rawOption,
            };
            invoicePaymentRemoveCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    long paymentId = parseResult.GetRequiredValue(invoicePaymentIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandlePaymentRemoveAsync(invoiceId, paymentId, raw, stamp, verbosity);
                });

            Command invoicePaymentAddBulkCommand = new("add-bulk", "Add payments for the full balance to multiple invoices at once with a shared payment date and details.")
            {
                stampOption,
                verbosityOption,
                rawOption,
            };
            invoicePaymentAddBulkCommand.Aliases.Add("bulk");
            invoicePaymentAddBulkCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandlePaymentAddBulkAsync(raw, stamp, verbosity);
                });

            invoicePaymentCommand.Subcommands.Add(invoicePaymentAddCommand);
            invoicePaymentCommand.Subcommands.Add(invoicePaymentEditCommand);
            invoicePaymentCommand.Subcommands.Add(invoicePaymentRemoveCommand);
            invoicePaymentCommand.Subcommands.Add(invoicePaymentAddBulkCommand);
            invoiceCommand.Subcommands.Add(invoicePaymentCommand);

            #endregion // Invoice Payments

            Command invoiceMarkReprocessRegularlyCommand = new("mark-reprocess-regularly", "Mark a specific invoice to be automatically reprocessed to pdf every 14d if not marked as paid.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                rawOption,
            };
            invoiceMarkReprocessRegularlyCommand.Aliases.Add("mrr");
            invoiceMarkReprocessRegularlyCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleMarkReprocessRegularlyAsync(invoiceId, raw, stamp, verbosity);
                });
            invoiceCommand.Subcommands.Add(invoiceMarkReprocessRegularlyCommand);

            Command invoiceProcessCommand = new("process", "Mark a specific invoice for [re-]processing to pdf.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                rawOption,
            };
            invoiceProcessCommand.Aliases.Add("reprocess");
            invoiceProcessCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleProcessAsync(invoiceId, raw, stamp, verbosity);
                });
            invoiceCommand.Subcommands.Add(invoiceProcessCommand);

            rootCommand.Subcommands.Add(invoiceCommand);

            #endregion // Invoice


            return rootCommand;
        }
    }
}
