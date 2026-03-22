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
            businessCreateCommand.AddCreateAliases();
            businessCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleBusinessCreateAsync(stamp, verbosity);
                });

            Command businessReadCommand = new("read", "Read an existing business.")
            {
                stampOption,
                verbosityOption,
                businessIdArgument,
            };
            businessReadCommand.AddReadAliases();
            businessReadCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long businessId = parseResult.GetRequiredValue(businessIdArgument);
                    return HandleBusinessGetAsync(businessId, stamp, verbosity);
                });

            Command businessListCommand = new("list", "List existing businesses.")
            {
                stampOption,
                verbosityOption,
            };
            businessListCommand.AddListAliases();
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
            businessLogoCreateCommand.AddCreateAliases();
            businessLogoCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long businessId = parseResult.GetRequiredValue(businessIdArgument);
                    FileInfo logoFileInfo = parseResult.GetRequiredValue(logoFileInfoArgument);
                    return HandleBusinessLogoCreateAsync(businessId, logoFileInfo, stamp, verbosity);
                });

            Command businessLogoUpdateCommand = new("update", "Update the logo for an existing business.")
            {
                stampOption,
                verbosityOption,
                businessIdArgument,
                logoFileInfoArgument,
            };
            businessLogoUpdateCommand.AddUpdateAliases();
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
            Command businessLogoReadCommand = new("read", "Download the logo for an existing business.")
            {
                stampOption,
                verbosityOption,
                businessIdArgument,
                outputFileInfoArgument,
            };
            businessLogoReadCommand.AddReadAliases();
            businessLogoReadCommand.SetAction(
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
            businessLogoDeleteCommand.AddDeleteAliases();
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
            businessLogoCommand.Subcommands.Add(businessLogoReadCommand);
            businessLogoCommand.Subcommands.Add(businessLogoDeleteCommand);
            businessCommand.Subcommands.Add(businessLogoCommand);

            businessCommand.Subcommands.Add(businessCreateCommand);
            businessCommand.Subcommands.Add(businessReadCommand);
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
            customerCreateCommand.AddCreateAliases();
            customerCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleCustomerCreateAsync(stamp, verbosity);
                });

            Command customerUpdateCommand = new("update", "Update an existing customer.")
            {
                stampOption,
                verbosityOption,
                customerIdArgument,
            };
            customerUpdateCommand.AddUpdateAliases();
            customerUpdateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long customerId = parseResult.GetRequiredValue(customerIdArgument);
                    return HandleCustomerEditAsync(customerId, stamp, verbosity);
                });

            Command customerReadCommand = new("read", "Read an existing customer.")
            {
                stampOption,
                verbosityOption,
                customerIdArgument,
            };
            customerReadCommand.AddReadAliases();
            customerReadCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long customerId = parseResult.GetRequiredValue(customerIdArgument);
                    return HandleCustomerGetAsync(customerId, stamp, verbosity);
                });

            Command customerListCommand = new("list", "List existing customers.")
            {
                stampOption,
                verbosityOption,
            };
            customerListCommand.AddListAliases();
            customerListCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleCustomerListAsync(stamp, verbosity);
                });

            customerCommand.Subcommands.Add(customerCreateCommand);
            customerCommand.Subcommands.Add(customerUpdateCommand);
            customerCommand.Subcommands.Add(customerReadCommand);
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
            statementCreateCommand.AddCreateAliases();
            statementCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleStatementCreateAsync(stamp, verbosity);
                });

            Command statementReadCommand = new("read", "Read an existing statement.")
            {
                stampOption,
                verbosityOption,
                statementIdArgument,
            };
            statementReadCommand.AddReadAliases();
            statementReadCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long statementId = parseResult.GetRequiredValue(statementIdArgument);
                    return HandleStatementGetAsync(statementId, stamp, verbosity);
                });

            Command statementListCommand = new("list", "List existing statements.")
            {
                stampOption,
                verbosityOption,
            };
            statementListCommand.AddListAliases();
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
            statementCommand.Subcommands.Add(statementReadCommand);
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
            invoiceLineCommand.Aliases.Add("ln");

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
            invoiceLineCreateCommand.AddCreateAliases();
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
            invoiceLineDuplicateCommand.Aliases.Add("copy");
            invoiceLineDuplicateCommand.Aliases.Add("cp");
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

            Command invoiceLineUpdateCommand = new("update", "Updatw an existing invoice line.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                invoiceLineIdArgument,
                rawOption,
            };
            invoiceLineUpdateCommand.AddUpdateAliases();
            invoiceLineUpdateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    long lineId = parseResult.GetRequiredValue(invoiceLineIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleLineEditAsync(invoiceId, lineId, raw, stamp, verbosity);
                });

            Command invoiceLineReadCommand = new("read", "Read an existing invoice line.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                invoiceLineIdArgument,
            };
            invoiceLineReadCommand.AddReadAliases();
            invoiceLineReadCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    long lineId = parseResult.GetRequiredValue(invoiceLineIdArgument);
                    return HandleLineGetAsync(invoiceId, lineId, stamp, verbosity);
                });

            Command invoiceLineListCommand = new("list", "List existing invoice lines.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
            };
            invoiceLineListCommand.AddListAliases();
            invoiceLineListCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    return HandleLineListAsync(invoiceId, stamp, verbosity);
                });

            Command invoiceLineDeleteCommand = new("delete", "Delete an existing invoice line.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                invoiceLineIdArgument,
                rawOption,
            };
            invoiceLineDeleteCommand.AddDeleteAliases();
            invoiceLineDeleteCommand.SetAction(
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
            invoiceLineCommand.Subcommands.Add(invoiceLineUpdateCommand);
            invoiceLineCommand.Subcommands.Add(invoiceLineReadCommand);
            invoiceLineCommand.Subcommands.Add(invoiceLineListCommand);
            invoiceLineCommand.Subcommands.Add(invoiceLineDeleteCommand);
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
            invoiceLineDefaultCreateCommand.AddCreateAliases();
            invoiceLineDefaultCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    return HandleLineDefaultCreateAsync(stamp, verbosity);
                });

            Command invoiceLineDefaultUpdateCommand = new("update", "Update an existing invoice line default.")
            {
                stampOption,
                verbosityOption,
                invoiceLineDefaultIdArgument,
            };
            invoiceLineDefaultUpdateCommand.AddUpdateAliases();
            invoiceLineDefaultUpdateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long id = parseResult.GetRequiredValue(invoiceLineDefaultIdArgument);
                    return HandleLineDefaultEditAsync(id, stamp, verbosity);
                });

            Command invoiceLineDefaultReadCommand = new("read", "Read an existing invoice line default.")
            {
                stampOption,
                verbosityOption,
                invoiceLineDefaultIdArgument,
            };
            invoiceLineDefaultReadCommand.AddReadAliases();
            invoiceLineDefaultReadCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long id = parseResult.GetRequiredValue(invoiceLineDefaultIdArgument);
                    return HandleLineDefaultGetAsync(id, stamp, verbosity);
                });

            Command invoiceLineDefaultListCommand = new("list", "List existing invoice line defaults.")
            {
                stampOption,
                verbosityOption,
            };
            invoiceLineDefaultListCommand.AddListAliases();
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
            invoiceLineDefaultCommand.Subcommands.Add(invoiceLineDefaultUpdateCommand);
            invoiceLineDefaultCommand.Subcommands.Add(invoiceLineDefaultReadCommand);
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
            createCommand.AddCreateAliases();
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

            Command invoiceUpdateCommand = new("update", "Update an existing invoice.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                rawOption,
            };
            invoiceUpdateCommand.AddUpdateAliases();
            invoiceUpdateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleEditAsync(invoiceId, raw, stamp, verbosity);
                });
            invoiceCommand.Subcommands.Add(invoiceUpdateCommand);

            Command invoiceReadCommand = new("read", "Read an existing invoice.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                rawOption,
            };
            invoiceReadCommand.AddReadAliases();
            invoiceReadCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandleGetAsync(invoiceId, raw, stamp, verbosity);
                });
            invoiceCommand.Subcommands.Add(invoiceReadCommand);

            Command invoiceListCommand = new("list", "List existing invoices.")
            {
                stampOption,
                verbosityOption,
            };
            invoiceListCommand.AddListAliases();
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

            Command invoicePaymentCreateCommand = new("Create", "Create a payment for an existing invoice.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                rawOption,
            };
            invoicePaymentCreateCommand.AddCreateAliases();
            invoicePaymentCreateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandlePaymentAddAsync(invoiceId, raw, stamp, verbosity);
                });

            Command invoicePaymentUpdateCommand = new("update", "Update the date or details of an existing payment.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                invoicePaymentIdArgument,
                rawOption,
            };
            invoicePaymentUpdateCommand.AddUpdateAliases();
            invoicePaymentUpdateCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    long invoiceId = parseResult.GetRequiredValue(invoiceIdArgument);
                    long paymentId = parseResult.GetRequiredValue(invoicePaymentIdArgument);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandlePaymentEditAsync(invoiceId, paymentId, raw, stamp, verbosity);
                });

            Command invoicePaymentDeleteCommand = new("delete", "Delete a payment from an existing invoice.")
            {
                stampOption,
                verbosityOption,
                invoiceIdArgument,
                invoicePaymentIdArgument,
                rawOption,
            };
            invoicePaymentDeleteCommand.AddDeleteAliases();
            invoicePaymentDeleteCommand.SetAction(
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
            invoicePaymentAddBulkCommand.Aliases.Add("create-bulk");
            invoicePaymentAddBulkCommand.SetAction(
                (parseResult, cancellationToken) =>
                {
                    Stamp stamp = parseResult.GetRequiredValue(stampOption);
                    Verbosity verbosity = parseResult.GetRequiredValue(verbosityOption);
                    bool raw = parseResult.GetValue(rawOption);
                    return HandlePaymentAddBulkAsync(raw, stamp, verbosity);
                });

            invoicePaymentCommand.Subcommands.Add(invoicePaymentCreateCommand);
            invoicePaymentCommand.Subcommands.Add(invoicePaymentUpdateCommand);
            invoicePaymentCommand.Subcommands.Add(invoicePaymentDeleteCommand);
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
