using System.CommandLine;
using TcpWtf.NumberSequence.Client;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class InvoiceCommand
    {
        public static Command Create(Option<Stamp> stampOption, Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("invoice", "Create and manage invoices.");


            Command businessCommand = new("business", "Manage invoice businesses (location that creates invoices).");

            Command businessCreateCommand = new("create", "Create a new invoice business.");
            businessCreateCommand.SetHandler(HandleBusinessCreateAsync, stampOption, verbosityOption);

            Command businessGetCommand = new("get", "Get an existing invoice business.");
            businessGetCommand.AddAlias("show");
            Argument<long> businessIdArgument = new("businessId", "The id of the business.");
            businessGetCommand.AddArgument(businessIdArgument);
            businessGetCommand.SetHandler(HandleBusinessGetAsync, businessIdArgument, stampOption, verbosityOption);

            Command businessListCommand = new("list", "Get existing invoice businesses.");
            businessListCommand.AddAlias("ls");
            businessListCommand.SetHandler(HandleBusinessListAsync, stampOption, verbosityOption);

            businessCommand.AddCommand(businessCreateCommand);
            businessCommand.AddCommand(businessGetCommand);
            businessCommand.AddCommand(businessListCommand);
            rootCommand.AddCommand(businessCommand);


            Command customerCommand = new("customer", "Manage invoice customers (location that receives invoices).");

            Command customerCreateCommand = new("create", "Create a new invoice customer.");
            customerCreateCommand.SetHandler(HandleCustomerCreateAsync, stampOption, verbosityOption);

            Command customerEditCommand = new("edit", "Edit an existing invoice customer.");
            Argument<long> customerIdArgument = new("customerId", "The id of the customer.");
            customerEditCommand.AddArgument(customerIdArgument);
            customerEditCommand.SetHandler(HandleCustomerEditAsync, customerIdArgument, stampOption, verbosityOption);

            Command customerGetCommand = new("get", "Get an existing invoice customer.");
            customerGetCommand.AddAlias("show");
            customerGetCommand.AddArgument(customerIdArgument);
            customerGetCommand.SetHandler(HandleCustomerGetAsync, customerIdArgument, stampOption, verbosityOption);

            Command customerListCommand = new("list", "Get existing invoice customers.");
            customerListCommand.AddAlias("ls");
            customerListCommand.SetHandler(HandleCustomerListAsync, stampOption, verbosityOption);

            customerCommand.AddCommand(customerCreateCommand);
            customerCommand.AddCommand(customerEditCommand);
            customerCommand.AddCommand(customerGetCommand);
            customerCommand.AddCommand(customerListCommand);
            rootCommand.AddCommand(customerCommand);


            Command lineDefaultCommand = new("line-default", "Manage invoice line defaults (line items that can be used as a reference for adding a line to an invoice).");

            Command lineDefaultCreateCommand = new("create", "Create a new invoice line default.");
            lineDefaultCreateCommand.SetHandler(HandleLineDefaultCreateAsync, stampOption, verbosityOption);

            Command lineDefaultEditCommand = new("edit", "Edit an existing invoice line.");
            Argument<long> lineDefaultIdArgument = new("lineDefaultId", "The id of the line default.");
            lineDefaultEditCommand.AddArgument(lineDefaultIdArgument);
            lineDefaultEditCommand.SetHandler(HandleLineDefaultEditAsync, lineDefaultIdArgument, stampOption, verbosityOption);

            Command lineDefaultGetCommand = new("get", "Get an existing invoice line default.");
            lineDefaultGetCommand.AddAlias("show");
            lineDefaultGetCommand.AddArgument(lineDefaultIdArgument);
            lineDefaultGetCommand.SetHandler(HandleLineDefaultGetAsync, lineDefaultIdArgument, stampOption, verbosityOption);

            Command lineDefaultListCommand = new("list", "Get existing invoice line defaults.");
            lineDefaultListCommand.AddAlias("ls");
            lineDefaultListCommand.SetHandler(HandleLineDefaultListAsync, stampOption, verbosityOption);

            Command lineDefaultSwapCommand = new("swap", "Since line defaults are a CLI utility, swap two of them for more preferred sorting.");
            Argument<long> lineDefaultOtherIdArgument = new("lineDefaultOtherId", "The id of the other line default.");
            lineDefaultSwapCommand.AddArgument(lineDefaultIdArgument);
            lineDefaultSwapCommand.AddArgument(lineDefaultOtherIdArgument);
            lineDefaultSwapCommand.SetHandler(HandleLineDefaultSwapAsync, lineDefaultIdArgument, lineDefaultOtherIdArgument, stampOption, verbosityOption);

            lineDefaultCommand.AddCommand(lineDefaultCreateCommand);
            lineDefaultCommand.AddCommand(lineDefaultEditCommand);
            lineDefaultCommand.AddCommand(lineDefaultGetCommand);
            lineDefaultCommand.AddCommand(lineDefaultListCommand);
            lineDefaultCommand.AddCommand(lineDefaultSwapCommand);
            rootCommand.AddCommand(lineDefaultCommand);


            Command lineCommand = new("line", "Manage invoice lines.");

            Command lineCreateCommand = new("create", "Create a new invoice line.");
            Argument<long> idArgument = new("invoiceId", "The id of the invoice.");
            Option<bool> rawOption = new("--raw", "Show raw json object(s) instead of the nicer summary format.");
            lineCreateCommand.AddArgument(idArgument);
            lineCreateCommand.AddOption(rawOption);
            lineCreateCommand.SetHandler(HandleLineCreateAsync, idArgument, rawOption, stampOption, verbosityOption);

            Command lineDuplicateCommand = new("duplicate", "Duplicate an invoice line, optionally to another invoice.");
            lineDuplicateCommand.AddAlias("dupe");
            Argument<long> lineIdArgument = new("lineId", "The id of the line.");
            Argument<long> targetIdArgument = new("target-invoice-id", () => -1, "If supplied, the target invoice id to create the duplicate line on.");
            lineDuplicateCommand.AddArgument(idArgument);
            lineDuplicateCommand.AddArgument(lineIdArgument);
            lineDuplicateCommand.AddArgument(targetIdArgument);
            lineDuplicateCommand.AddOption(rawOption);
            lineDuplicateCommand.SetHandler(HandleLineDuplicateAsync, idArgument, lineIdArgument, targetIdArgument, rawOption, stampOption, verbosityOption);

            Command lineEditCommand = new("edit", "Edit an existing invoice line.");
            lineEditCommand.AddArgument(idArgument);
            lineEditCommand.AddArgument(lineIdArgument);
            lineEditCommand.AddOption(rawOption);
            lineEditCommand.SetHandler(HandleLineEditAsync, idArgument, lineIdArgument, rawOption, stampOption, verbosityOption);

            Command lineGetCommand = new("get", "Get an existing invoice line.");
            lineGetCommand.AddAlias("show");
            lineGetCommand.AddArgument(idArgument);
            lineGetCommand.AddArgument(lineIdArgument);
            lineGetCommand.SetHandler(HandleLineGetAsync, idArgument, lineIdArgument, stampOption, verbosityOption);

            Command lineListCommand = new("list", "Get existing invoice lines.");
            lineListCommand.AddAlias("ls");
            lineListCommand.AddArgument(idArgument);
            lineListCommand.SetHandler(HandleLineListAsync, idArgument, stampOption, verbosityOption);

            Command lineRemoveCommand = new("remove", "Remove an existing invoice line.");
            lineRemoveCommand.AddArgument(idArgument);
            lineRemoveCommand.AddArgument(lineIdArgument);
            lineRemoveCommand.AddOption(rawOption);
            lineRemoveCommand.SetHandler(HandleLineRemoveAsync, idArgument, lineIdArgument, rawOption, stampOption, verbosityOption);

            Command lineSwapCommand = new("swap", "Swap lines in an invoice to change their ordering to a more preferred sorting.");
            Argument<long> lineOtherIdArgument = new("lineOtherId", "The id of the other line.");
            lineSwapCommand.AddArgument(idArgument);
            lineSwapCommand.AddArgument(lineIdArgument);
            lineSwapCommand.AddArgument(lineOtherIdArgument);
            lineRemoveCommand.AddOption(rawOption);
            lineSwapCommand.SetHandler(HandleLineSwapAsync, idArgument, lineIdArgument, lineOtherIdArgument, rawOption, stampOption, verbosityOption);

            lineCommand.AddCommand(lineCreateCommand);
            lineCommand.AddCommand(lineDuplicateCommand);
            lineCommand.AddCommand(lineEditCommand);
            lineCommand.AddCommand(lineGetCommand);
            lineCommand.AddCommand(lineListCommand);
            lineCommand.AddCommand(lineRemoveCommand);
            lineCommand.AddCommand(lineSwapCommand);
            rootCommand.AddCommand(lineCommand);


            Command createCommand = new("create", "Create a new invoice.");
            Option<long?> idFromOption = new(
                "--from",
                "The id of the invoice if creating from another invoice. " +
                "The first two lines of the new invoice will be the total of the previous and the payment information.");
            createCommand.AddOption(idFromOption);
            createCommand.SetHandler(HandleCreateAsync, idFromOption, rawOption, stampOption, verbosityOption);
            rootCommand.AddCommand(createCommand);


            Command editCommand = new("edit", "Edit an existing invoice.");
            editCommand.AddArgument(idArgument);
            editCommand.SetHandler(HandleEditAsync, idArgument, rawOption, stampOption, verbosityOption);
            rootCommand.AddCommand(editCommand);


            Command getCommand = new("get", "Get an existing invoice.");
            getCommand.AddAlias("show");
            getCommand.AddArgument(idArgument);
            getCommand.AddOption(rawOption);
            getCommand.SetHandler(HandleGetAsync, idArgument, rawOption, stampOption, verbosityOption);
            rootCommand.AddCommand(getCommand);


            Command listCommand = new("list", "Get existing invoices.");
            listCommand.AddAlias("ls");
            listCommand.SetHandler(HandleListAsync, stampOption, verbosityOption);
            rootCommand.AddCommand(listCommand);


            Command markPaidCommand = new("mark-paid", "Mark a specific invoice as paid.");
            markPaidCommand.AddArgument(idArgument);
            markPaidCommand.SetHandler(HandleMarkPaidAsync, idArgument, rawOption, stampOption, verbosityOption);
            rootCommand.AddCommand(markPaidCommand);


            Command markReprocessRegularlyCommand = new("mark-reprocess-regularly", "Mark a specific invoice to be automatically reprocessed to pdf every 14d if not marked as paid.");
            markReprocessRegularlyCommand.AddArgument(idArgument);
            markReprocessRegularlyCommand.SetHandler(HandleMarkReprocessRegularlyAsync, idArgument, rawOption, stampOption, verbosityOption);
            rootCommand.AddCommand(markReprocessRegularlyCommand);


            Command processCommand = new("process", "Mark a specific invoice for [re-]processing to pdf.");
            processCommand.AddAlias("reprocess");
            processCommand.AddArgument(idArgument);
            processCommand.SetHandler(HandleProcessAsync, idArgument, rawOption, stampOption, verbosityOption);
            rootCommand.AddCommand(processCommand);


            return rootCommand;
        }
    }
}
