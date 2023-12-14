using System.CommandLine;
using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static class InvoiceCommand
    {
        public static Command Create(Option<Verbosity> verbosityOption)
        {
            Command rootCommand = new("invoice", "Create and manage invoices.");


            Command businessCommand = new("business", "Manage invoice businesses (location that creates invoices).");

            Command businessCreateCommand = new("create", "Create a new invoice business.");
            businessCreateCommand.SetHandler(HandleBusinessCreateAsync, verbosityOption);

            Command businessGetCommand = new("get", "Get an existing invoice business.");
            businessGetCommand.AddAlias("show");
            Argument<long> businessIdArgument = new("businessId", "The id of the business.");
            businessGetCommand.AddArgument(businessIdArgument);
            businessGetCommand.SetHandler(HandleBusinessGetAsync, businessIdArgument, verbosityOption);

            Command businessListCommand = new("list", "Get existing invoice businesses.");
            businessListCommand.AddAlias("ls");
            businessListCommand.SetHandler(HandleBusinessListAsync, verbosityOption);

            businessCommand.AddCommand(businessCreateCommand);
            businessCommand.AddCommand(businessGetCommand);
            businessCommand.AddCommand(businessListCommand);
            rootCommand.AddCommand(businessCommand);


            Command customerCommand = new("customer", "Manage invoice customers (location that receives invoices).");

            Command customerCreateCommand = new("create", "Create a new invoice customer.");
            customerCreateCommand.SetHandler(HandleCustomerCreateAsync, verbosityOption);

            Command customerEditCommand = new("edit", "Edit an existing invoice customer.");
            Argument<long> customerIdArgument = new("customerId", "The id of the customer.");
            customerEditCommand.AddArgument(customerIdArgument);
            customerEditCommand.SetHandler(HandleCustomerEditAsync, customerIdArgument, verbosityOption);

            Command customerGetCommand = new("get", "Get an existing invoice customer.");
            customerGetCommand.AddAlias("show");
            customerGetCommand.AddArgument(customerIdArgument);
            customerGetCommand.SetHandler(HandleCustomerGetAsync, customerIdArgument, verbosityOption);

            Command customerListCommand = new("list", "Get existing invoice customers.");
            customerListCommand.AddAlias("ls");
            customerListCommand.SetHandler(HandleCustomerListAsync, verbosityOption);

            customerCommand.AddCommand(customerCreateCommand);
            customerCommand.AddCommand(customerEditCommand);
            customerCommand.AddCommand(customerGetCommand);
            customerCommand.AddCommand(customerListCommand);
            rootCommand.AddCommand(customerCommand);


            Command lineDefaultCommand = new("line-default", "Manage invoice line defaults (line items that can be used as a reference for adding a line to an invoice).");

            Command lineDefaultCreateCommand = new("create", "Create a new invoice line default.");
            lineDefaultCreateCommand.SetHandler(HandleLineDefaultCreateAsync, verbosityOption);

            Command lineDefaultEditCommand = new("edit", "Edit an existing invoice line.");
            Argument<long> lineDefaultIdArgument = new("lineDefaultId", "The id of the line default.");
            lineDefaultEditCommand.AddArgument(lineDefaultIdArgument);
            lineDefaultEditCommand.SetHandler(HandleLineDefaultEditAsync, lineDefaultIdArgument, verbosityOption);

            Command lineDefaultGetCommand = new("get", "Get an existing invoice line default.");
            lineDefaultGetCommand.AddAlias("show");
            lineDefaultGetCommand.AddArgument(lineDefaultIdArgument);
            lineDefaultGetCommand.SetHandler(HandleLineDefaultGetAsync, lineDefaultIdArgument, verbosityOption);

            Command lineDefaultListCommand = new("list", "Get existing invoice line defaults.");
            lineDefaultListCommand.AddAlias("ls");
            lineDefaultListCommand.SetHandler(HandleLineDefaultListAsync, verbosityOption);

            lineDefaultCommand.AddCommand(lineDefaultCreateCommand);
            lineDefaultCommand.AddCommand(lineDefaultEditCommand);
            lineDefaultCommand.AddCommand(lineDefaultGetCommand);
            lineDefaultCommand.AddCommand(lineDefaultListCommand);
            rootCommand.AddCommand(lineDefaultCommand);


            Command lineCommand = new("line", "Manage invoice lines.");

            Command lineCreateCommand = new("create", "Create a new invoice line.");
            Argument<long> idArgument = new("invoiceId", "The id of the invoice.");
            Option<bool> rawOption = new("--raw", "Show raw json object(s) instead of the nicer summary format.");
            lineCreateCommand.AddArgument(idArgument);
            lineCreateCommand.AddOption(rawOption);
            lineCreateCommand.SetHandler(HandleLineCreateAsync, idArgument, rawOption, verbosityOption);

            Command lineDuplicateCommand = new("duplicate", "Duplicate an invoice line, optionally to another invoice.");
            lineDuplicateCommand.AddAlias("dupe");
            Argument<long> lineIdArgument = new("lineId", "The id of the line.");
            Argument<long> targetIdArgument = new("target-invoice-id", () => -1, "If supplied, the target invoice id to create the duplicate line on.");
            lineDuplicateCommand.AddArgument(idArgument);
            lineDuplicateCommand.AddArgument(lineIdArgument);
            lineDuplicateCommand.AddArgument(targetIdArgument);
            lineDuplicateCommand.AddOption(rawOption);
            lineDuplicateCommand.SetHandler(HandleLineDuplicateAsync, idArgument, lineIdArgument, targetIdArgument, rawOption, verbosityOption);

            Command lineEditCommand = new("edit", "Edit an existing invoice line.");
            lineEditCommand.AddArgument(idArgument);
            lineEditCommand.AddArgument(lineIdArgument);
            lineEditCommand.AddOption(rawOption);
            lineEditCommand.SetHandler(HandleLineEditAsync, idArgument, lineIdArgument, rawOption, verbosityOption);

            Command lineGetCommand = new("get", "Get an existing invoice line.");
            lineGetCommand.AddAlias("show");
            lineGetCommand.AddArgument(idArgument);
            lineGetCommand.AddArgument(lineIdArgument);
            lineGetCommand.SetHandler(HandleLineGetAsync, idArgument, lineIdArgument, verbosityOption);

            Command lineListCommand = new("list", "Get existing invoice lines.");
            lineListCommand.AddAlias("ls");
            lineListCommand.AddArgument(idArgument);
            lineListCommand.SetHandler(HandleLineListAsync, idArgument, verbosityOption);

            Command lineRemoveCommand = new("remove", "Remove an existing invoice line.");
            lineRemoveCommand.AddArgument(idArgument);
            lineRemoveCommand.AddArgument(lineIdArgument);
            lineRemoveCommand.AddOption(rawOption);
            lineRemoveCommand.SetHandler(HandleLineRemoveAsync, idArgument, lineIdArgument, rawOption, verbosityOption);

            lineCommand.AddCommand(lineCreateCommand);
            lineCommand.AddCommand(lineDuplicateCommand);
            lineCommand.AddCommand(lineEditCommand);
            lineCommand.AddCommand(lineGetCommand);
            lineCommand.AddCommand(lineListCommand);
            lineCommand.AddCommand(lineRemoveCommand);
            rootCommand.AddCommand(lineCommand);


            Command createCommand = new("create", "Create a new invoice.");
            Option<long?> idFromOption = new(
                "--from",
                "The id of the invoice if creating from another invoice. " +
                "The first two lines of the new invoice will be the total of the previous and the payment information.");
            createCommand.AddOption(idFromOption);
            createCommand.SetHandler(HandleCreateAsync, idFromOption, rawOption, verbosityOption);
            rootCommand.AddCommand(createCommand);


            Command editCommand = new("edit", "Edit an existing invoice.");
            editCommand.AddArgument(idArgument);
            editCommand.SetHandler(HandleEditAsync, idArgument, rawOption, verbosityOption);
            rootCommand.AddCommand(editCommand);


            Command getCommand = new("get", "Get an existing invoice.");
            getCommand.AddAlias("show");
            getCommand.AddArgument(idArgument);
            getCommand.AddOption(rawOption);
            getCommand.SetHandler(HandleGetAsync, idArgument, rawOption, verbosityOption);
            rootCommand.AddCommand(getCommand);


            Command listCommand = new("list", "Get existing invoices.");
            listCommand.AddAlias("ls");
            listCommand.SetHandler(HandleListAsync, verbosityOption);
            rootCommand.AddCommand(listCommand);


            Command markPaidCommand = new("mark-paid", "Mark a specific invoice as paid.");
            markPaidCommand.AddArgument(idArgument);
            markPaidCommand.SetHandler(HandleMarkPaidAsync, idArgument, rawOption, verbosityOption);
            rootCommand.AddCommand(markPaidCommand);


            Command markReprocessRegularlyCommand = new("mark-reprocess-regularly", "Mark a specific invoice to be automatically reprocessed to pdf every 14d if not marked as paid.");
            markReprocessRegularlyCommand.AddArgument(idArgument);
            markReprocessRegularlyCommand.SetHandler(HandleMarkReprocessRegularlyAsync, idArgument, rawOption, verbosityOption);
            rootCommand.AddCommand(markReprocessRegularlyCommand);


            Command processCommand = new("process", "Mark a specific invoice for [re-]processing to pdf.");
            processCommand.AddAlias("reprocess");
            processCommand.AddArgument(idArgument);
            processCommand.SetHandler(HandleProcessAsync, idArgument, rawOption, verbosityOption);
            rootCommand.AddCommand(processCommand);


            return rootCommand;
        }

        #region Businesses

        private static async Task HandleBusinessCreateAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.InvoiceBusiness invoiceBusiness = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Name = Input.GetString(nameof(invoiceBusiness.Name)),
                AddressLine1 = Input.GetString(nameof(invoiceBusiness.AddressLine1)),
                AddressLine2 = Input.GetString(nameof(invoiceBusiness.AddressLine2)),
                Contact = Input.GetString(nameof(invoiceBusiness.Contact)),
            };

            invoiceBusiness = await client.Invoice.CreateBusinessAsync(invoiceBusiness);
            Console.WriteLine(invoiceBusiness.ToJsonString(indented: true));
        }

        private static async Task HandleBusinessGetAsync(long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.InvoiceBusiness invoiceBusiness = await client.Invoice.GetBusinessAsync(id);
            Console.WriteLine(invoiceBusiness.ToJsonString(indented: true));
        }

        private static async Task HandleBusinessListAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            List<Contracts.Invoicing.InvoiceBusiness> invoiceBusinesses = await client.Invoice.GetBusinessesAsync();

            Console.WriteLine();
            Output.WriteTable(
                invoiceBusinesses,
                nameof(Contracts.Invoicing.InvoiceBusiness.Id),
                nameof(Contracts.Invoicing.InvoiceBusiness.Name),
                nameof(Contracts.Invoicing.InvoiceBusiness.AddressLine1),
                nameof(Contracts.Invoicing.InvoiceBusiness.AddressLine2),
                nameof(Contracts.Invoicing.InvoiceBusiness.Contact),
                nameof(Contracts.Invoicing.InvoiceBusiness.CreatedDate));
        }

        #endregion // Businesses

        #region Customers

        private static async Task HandleCustomerCreateAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.InvoiceCustomer invoiceCustomer = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Name = Input.GetString(nameof(invoiceCustomer.Name)),
                AddressLine1 = Input.GetString(nameof(invoiceCustomer.AddressLine1)),
                AddressLine2 = Input.GetString(nameof(invoiceCustomer.AddressLine2)),
                Contact = Input.GetString(nameof(invoiceCustomer.Contact)),
            };

            invoiceCustomer = await client.Invoice.CreateCustomerAsync(invoiceCustomer);
            Console.WriteLine(invoiceCustomer.ToJsonString(indented: true));
        }

        private static async Task HandleCustomerEditAsync(long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.InvoiceCustomer invoiceCustomer = await client.Invoice.GetCustomerAsync(id);

            invoiceCustomer.Name = Input.GetString(nameof(invoiceCustomer.Name), invoiceCustomer.Name);
            invoiceCustomer.AddressLine1 = Input.GetString(nameof(invoiceCustomer.AddressLine1), invoiceCustomer.AddressLine1);
            invoiceCustomer.AddressLine2 = Input.GetString(nameof(invoiceCustomer.AddressLine2), invoiceCustomer.AddressLine2);
            invoiceCustomer.Contact = Input.GetString(nameof(invoiceCustomer.Contact), invoiceCustomer.Contact);

            invoiceCustomer = await client.Invoice.UpdateCustomerAsync(invoiceCustomer);
            Console.WriteLine(invoiceCustomer.ToJsonString(indented: true));
        }

        private static async Task HandleCustomerGetAsync(long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.InvoiceCustomer invoiceCustomer = await client.Invoice.GetCustomerAsync(id);
            Console.WriteLine(invoiceCustomer.ToJsonString(indented: true));
        }

        private static async Task HandleCustomerListAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            List<Contracts.Invoicing.InvoiceCustomer> invoiceCustomers = await client.Invoice.GetCustomersAsync();

            Console.WriteLine();
            Output.WriteTable(
                invoiceCustomers,
                nameof(Contracts.Invoicing.InvoiceCustomer.Id),
                nameof(Contracts.Invoicing.InvoiceCustomer.Name),
                nameof(Contracts.Invoicing.InvoiceCustomer.AddressLine1),
                nameof(Contracts.Invoicing.InvoiceCustomer.AddressLine2),
                nameof(Contracts.Invoicing.InvoiceCustomer.Contact),
                nameof(Contracts.Invoicing.InvoiceCustomer.CreatedDate));
        }

        #endregion // Customers

        #region Line Defaults

        private static async Task HandleLineDefaultCreateAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.InvoiceLineDefault invoiceLineDefault = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Title = Input.GetString(nameof(invoiceLineDefault.Title)),
                Description = Input.GetString(nameof(invoiceLineDefault.Description)),
                Quantity = Input.GetDecimal(nameof(invoiceLineDefault.Quantity), defaultVal: 1),
                Unit = Input.GetString(nameof(invoiceLineDefault.Unit)),
                Price = Input.GetDecimal(nameof(invoiceLineDefault.Price)),
            };

            invoiceLineDefault = await client.Invoice.CreateLineDefaultAsync(invoiceLineDefault);
            Console.WriteLine(invoiceLineDefault.ToJsonString(indented: true));
        }

        private static async Task HandleLineDefaultEditAsync(long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.InvoiceLineDefault invoiceLineDefault = await client.Invoice.GetLineDefaultAsync(id);

            invoiceLineDefault.Title = Input.GetString(nameof(invoiceLineDefault.Title), invoiceLineDefault.Title);
            invoiceLineDefault.Description = Input.GetString(nameof(invoiceLineDefault.Description), invoiceLineDefault.Description);
            invoiceLineDefault.Quantity = Input.GetDecimal(nameof(invoiceLineDefault.Quantity), defaultVal: invoiceLineDefault.Quantity);
            invoiceLineDefault.Unit = Input.GetString(nameof(invoiceLineDefault.Unit), invoiceLineDefault.Unit);
            invoiceLineDefault.Price = Input.GetDecimal(nameof(invoiceLineDefault.Price), defaultVal: invoiceLineDefault.Price);

            invoiceLineDefault = await client.Invoice.UpdateLineDefaultAsync(invoiceLineDefault);
            Console.WriteLine(invoiceLineDefault.ToJsonString(indented: true));
        }

        private static async Task HandleLineDefaultGetAsync(long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.InvoiceLineDefault invoiceLineDefault = await client.Invoice.GetLineDefaultAsync(id);
            Console.WriteLine(invoiceLineDefault.ToJsonString(indented: true));
        }

        private static async Task HandleLineDefaultListAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            List<Contracts.Invoicing.InvoiceLineDefault> invoiceLineDefaults = await client.Invoice.GetLineDefaultsAsync();

            Console.WriteLine();
            Output.WriteTable(
                invoiceLineDefaults,
                nameof(Contracts.Invoicing.InvoiceLineDefault.Id),
                nameof(Contracts.Invoicing.InvoiceLineDefault.Title),
                nameof(Contracts.Invoicing.InvoiceLineDefault.Description),
                nameof(Contracts.Invoicing.InvoiceLineDefault.Quantity),
                nameof(Contracts.Invoicing.InvoiceLineDefault.Unit),
                nameof(Contracts.Invoicing.InvoiceLineDefault.Price),
                nameof(Contracts.Invoicing.InvoiceLineDefault.ModifiedDate));
        }

        #endregion // Line Defaults

        #region Lines

        private static async Task HandleLineCreateAsync(long invoiceId, bool raw, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(invoiceId);
            PrintSingleInvoice(invoice, raw);

            List<Contracts.Invoicing.InvoiceLineDefault> invoiceLineDefaults = await client.Invoice.GetLineDefaultsAsync();
            invoiceLineDefaults.Insert(0, new() { Id = 0, Title = "Non-default entry" });
            Console.WriteLine("Invoice line defaults:");
            Output.WriteTable(
                invoiceLineDefaults,
                nameof(Contracts.Invoicing.InvoiceLineDefault.Id),
                nameof(Contracts.Invoicing.InvoiceLineDefault.Title),
                nameof(Contracts.Invoicing.InvoiceLineDefault.Description),
                nameof(Contracts.Invoicing.InvoiceLineDefault.Quantity),
                nameof(Contracts.Invoicing.InvoiceLineDefault.Unit),
                nameof(Contracts.Invoicing.InvoiceLineDefault.Price),
                nameof(Contracts.Invoicing.InvoiceLineDefault.ModifiedDate));

            long invoiceLineDefaultId = Input.GetLong("InvoiceLineDefault.Id");
            Contracts.Invoicing.InvoiceLineDefault invoiceLineDefault = invoiceLineDefaults.Skip(1).SingleOrDefault(x => x.Id == invoiceLineDefaultId);
            Contracts.Invoicing.InvoiceLine invoiceLine;
            invoiceLine = new()
            {
                Title = Input.GetString(nameof(invoiceLine.Title), invoiceLineDefault?.Title),
                Description = Input.GetString(nameof(invoiceLine.Description), invoiceLineDefault?.Description),
                Quantity = Input.GetDecimal(nameof(invoiceLine.Quantity), defaultVal: invoiceLineDefault?.Quantity ?? 1),
                Unit = Input.GetString(nameof(invoiceLine.Unit), invoiceLineDefault?.Unit),
                Price = Input.GetDecimal(nameof(invoiceLine.Price), defaultVal: invoiceLineDefault?.Price ?? 0),
            };

            invoice.Lines.Add(invoiceLine);
            invoice = await client.Invoice.UpdateAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleLineDuplicateAsync(long invoiceId, long id, long targetInvoiceId, bool raw, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.Invoice sourceInvoice = await client.Invoice.GetAsync(invoiceId);
            Contracts.Invoicing.InvoiceLine sourceInvoiceLine = sourceInvoice.Lines.Single(x => x.Id == id);

            Contracts.Invoicing.Invoice targetInvoice = targetInvoiceId == -1 ? sourceInvoice : await client.Invoice.GetAsync(targetInvoiceId);
            Contracts.Invoicing.InvoiceLine targetInvoiceLine;
            targetInvoiceLine = new()
            {
                Title = Input.GetString(nameof(sourceInvoiceLine.Title), sourceInvoiceLine.Title),
                Description = Input.GetString(nameof(sourceInvoiceLine.Description), sourceInvoiceLine.Description),
                Quantity = Input.GetDecimal(nameof(sourceInvoiceLine.Quantity), defaultVal: sourceInvoiceLine.Quantity),
                Unit = Input.GetString(nameof(sourceInvoiceLine.Unit), sourceInvoiceLine.Unit),
                Price = Input.GetDecimal(nameof(sourceInvoiceLine.Price), defaultVal: sourceInvoiceLine.Price),
            };

            targetInvoice.Lines.Add(targetInvoiceLine);
            targetInvoice = await client.Invoice.UpdateAsync(targetInvoice);
            PrintSingleInvoice(targetInvoice, raw);
        }

        private static async Task HandleLineEditAsync(long invoiceId, long id, bool raw, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(invoiceId);
            Contracts.Invoicing.InvoiceLine invoiceLine = invoice.Lines.Single(x => x.Id == id);

            invoiceLine.Title = Input.GetString(nameof(invoiceLine.Title), invoiceLine.Title);
            invoiceLine.Description = Input.GetString(nameof(invoiceLine.Description), invoiceLine.Description);
            invoiceLine.Quantity = Input.GetDecimal(nameof(invoiceLine.Quantity), defaultVal: invoiceLine.Quantity);
            invoiceLine.Unit = Input.GetString(nameof(invoiceLine.Unit), invoiceLine.Unit);
            invoiceLine.Price = Input.GetDecimal(nameof(invoiceLine.Price), defaultVal: invoiceLine.Price);

            invoice = await client.Invoice.UpdateAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleLineGetAsync(long invoiceId, long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(invoiceId);
            Contracts.Invoicing.InvoiceLine invoiceLine = invoice.Lines.Single(x => x.Id == id);
            Console.WriteLine(invoiceLine.ToJsonString(indented: true));
        }

        private static async Task HandleLineListAsync(long invoiceId, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(invoiceId);

            Console.WriteLine();
            Output.WriteTable(
                invoice.Lines,
                nameof(Contracts.Invoicing.InvoiceLine.Id),
                nameof(Contracts.Invoicing.InvoiceLine.Title),
                nameof(Contracts.Invoicing.InvoiceLine.Description),
                nameof(Contracts.Invoicing.InvoiceLine.Quantity),
                nameof(Contracts.Invoicing.InvoiceLine.Unit),
                nameof(Contracts.Invoicing.InvoiceLine.Price),
                nameof(Contracts.Invoicing.InvoiceLine.Total),
                nameof(Contracts.Invoicing.InvoiceLine.CreatedDate),
                nameof(Contracts.Invoicing.InvoiceLine.ModifiedDate));
        }

        private static async Task HandleLineRemoveAsync(long invoiceId, long id, bool raw, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(invoiceId);
            Contracts.Invoicing.InvoiceLine invoiceLine = invoice.Lines.Single(x => x.Id == id);
            _ = invoice.Lines.Remove(invoiceLine);
            Console.WriteLine(invoiceLine.ToJsonString(indented: true));

            invoice = await client.Invoice.UpdateAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }

        #endregion // Lines

        private static async Task HandleCreateAsync(long? fromId, bool raw, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.Invoice createFromInvoice = default;
            if (fromId.HasValue)
            {
                createFromInvoice = await client.Invoice.GetAsync(fromId.Value);
                if (!createFromInvoice.PaidDate.HasValue)
                {
                    throw new InvalidOperationException($"Create from invoice {createFromInvoice.Id} does not have a paid date. Creating invoices from another only supports invoices that are marked paid.");
                }
            }

            Contracts.Invoicing.Invoice invoice = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Title = Input.GetString(nameof(invoice.Title)),
                Description = Input.GetString(nameof(invoice.Description)),
                DueDate = Input.GetDateOnly(nameof(invoice.DueDate)),
            };

            List<Contracts.Invoicing.InvoiceBusiness> invoiceBusinesses = await client.Invoice.GetBusinessesAsync();
            Console.WriteLine("Invoice businesses:");
            Output.WriteTable(
                invoiceBusinesses,
                nameof(Contracts.Invoicing.InvoiceBusiness.Id),
                nameof(Contracts.Invoicing.InvoiceBusiness.Name),
                nameof(Contracts.Invoicing.InvoiceBusiness.AddressLine1),
                nameof(Contracts.Invoicing.InvoiceBusiness.AddressLine2),
                nameof(Contracts.Invoicing.InvoiceBusiness.Contact),
                nameof(Contracts.Invoicing.InvoiceBusiness.CreatedDate));
            long invoiceBusinessId = createFromInvoice == default
                ? Input.GetLong("Business.Id", canDefault: false)
                : Input.GetLong("Business.Id", canDefault: true, defaultVal: createFromInvoice.Business.Id);
            invoice.Business = invoiceBusinesses.Single(x => x.Id == invoiceBusinessId);

            List<Contracts.Invoicing.InvoiceCustomer> invoiceCustomers = await client.Invoice.GetCustomersAsync();
            Console.WriteLine("Invoice customers:");
            Output.WriteTable(
                invoiceCustomers,
                nameof(Contracts.Invoicing.InvoiceCustomer.Id),
                nameof(Contracts.Invoicing.InvoiceCustomer.Name),
                nameof(Contracts.Invoicing.InvoiceCustomer.AddressLine1),
                nameof(Contracts.Invoicing.InvoiceCustomer.AddressLine2),
                nameof(Contracts.Invoicing.InvoiceCustomer.Contact),
                nameof(Contracts.Invoicing.InvoiceCustomer.CreatedDate));
            long invoiceCustomerId = createFromInvoice == default
                ? Input.GetLong("Customer.Id", canDefault: false)
                : Input.GetLong("Customer.Id", canDefault: true, defaultVal: createFromInvoice.Customer.Id);
            invoice.Customer = invoiceCustomers.Single(x => x.Id == invoiceCustomerId);

            invoice = await client.Invoice.CreateAsync(invoice);

            if (createFromInvoice != default)
            {
                invoice.Lines ??= new List<Contracts.Invoicing.InvoiceLine>();
                invoice.Lines.Add(new()
                {
                    Title = $"Invoice \"{createFromInvoice.Title}\" (id {createFromInvoice.Id:0000}), due {createFromInvoice.DueDate:o}",
                    Description = createFromInvoice.Description,
                    Quantity = 1,
                    Price = createFromInvoice.Total,
                });
                invoice.Lines.Add(new()
                {
                    Title = $"Payment received on {createFromInvoice.PaidDate:o}",
                    Description = createFromInvoice.PaidDetails,
                    Quantity = 1,
                    Price = -createFromInvoice.Total,
                });
                invoice = await client.Invoice.UpdateAsync(invoice);
            }

            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleEditAsync(long id, bool raw, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(id);
            invoice.Title = Input.GetString(nameof(invoice.Title), invoice.Title);
            invoice.Description = Input.GetString(nameof(invoice.Description), invoice.Description);
            invoice.DueDate = Input.GetDateOnly(nameof(invoice.DueDate), invoice.DueDate);

            List<Contracts.Invoicing.InvoiceBusiness> invoiceBusinesses = await client.Invoice.GetBusinessesAsync();
            Console.WriteLine("Invoice businesses:");
            Output.WriteTable(
                invoiceBusinesses,
                nameof(Contracts.Invoicing.InvoiceBusiness.Id),
                nameof(Contracts.Invoicing.InvoiceBusiness.Name),
                nameof(Contracts.Invoicing.InvoiceBusiness.AddressLine1),
                nameof(Contracts.Invoicing.InvoiceBusiness.AddressLine2),
                nameof(Contracts.Invoicing.InvoiceBusiness.Contact),
                nameof(Contracts.Invoicing.InvoiceBusiness.CreatedDate));
            long invoiceBusinessId = Input.GetLong("Business.Id", defaultVal: invoice.Business.Id);
            invoice.Business = invoiceBusinesses.Single(x => x.Id == invoiceBusinessId);

            List<Contracts.Invoicing.InvoiceCustomer> invoiceCustomers = await client.Invoice.GetCustomersAsync();
            Console.WriteLine("Invoice customers:");
            Output.WriteTable(
                invoiceCustomers,
                nameof(Contracts.Invoicing.InvoiceCustomer.Id),
                nameof(Contracts.Invoicing.InvoiceCustomer.Name),
                nameof(Contracts.Invoicing.InvoiceCustomer.AddressLine1),
                nameof(Contracts.Invoicing.InvoiceCustomer.AddressLine2),
                nameof(Contracts.Invoicing.InvoiceCustomer.Contact),
                nameof(Contracts.Invoicing.InvoiceCustomer.CreatedDate));
            long invoiceCustomerId = Input.GetLong("Customer.Id", defaultVal: invoice.Customer.Id);
            invoice.Customer = invoiceCustomers.Single(x => x.Id == invoiceCustomerId);

            invoice = await client.Invoice.UpdateAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleGetAsync(long id, bool raw, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(id);

            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleListAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            List<Contracts.Invoicing.Invoice> invoices = await client.Invoice.GetAsync();

            Console.WriteLine();
            PrintInvoices(invoices.ToArray());
        }

        private static async Task HandleMarkPaidAsync(long id, bool raw, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(id);

            invoice.PaidDate = Input.GetDateOnly(nameof(invoice.PaidDate));
            invoice.PaidDetails = Input.GetString(nameof(invoice.PaidDetails));

            invoice = await client.Invoice.UpdateAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleMarkReprocessRegularlyAsync(long id, bool raw, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(id);

            invoice.ReprocessRegularly = true;

            invoice = await client.Invoice.UpdateAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleProcessAsync(long id, bool raw, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(id);

            invoice.ReadyForProcessing = true;
            invoice.ProcessedAt = default;

            invoice = await client.Invoice.UpdateAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }

        private static void PrintSingleInvoice(Contracts.Invoicing.Invoice invoice, bool raw)
        {
            if (raw)
            {
                Console.WriteLine(invoice.ToJsonString(indented: true));
            }
            else
            {
                // When displaying an invoice by default, nicely output the data instead of dumping the json out
                // Primarily for invoices with more lines (as it reduces the amount of scrolling necessary)
                Console.WriteLine();

                Console.WriteLine("Summary:");
                PrintInvoices(invoice);

                Console.WriteLine("Lines:");
                Output.WriteTable(
                    invoice.Lines,
                    nameof(Contracts.Invoicing.InvoiceLine.Id),
                    nameof(Contracts.Invoicing.InvoiceLine.Title),
                    nameof(Contracts.Invoicing.InvoiceLine.Description),
                    nameof(Contracts.Invoicing.InvoiceLine.Quantity),
                    nameof(Contracts.Invoicing.InvoiceLine.Unit),
                    nameof(Contracts.Invoicing.InvoiceLine.Price),
                    nameof(Contracts.Invoicing.InvoiceLine.Total),
                    nameof(Contracts.Invoicing.InvoiceLine.CreatedDate),
                    nameof(Contracts.Invoicing.InvoiceLine.ModifiedDate));
            }
        }

        private static void PrintInvoices(params Contracts.Invoicing.Invoice[] invoices)
        {
            Output.WriteTable(
                invoices.Select(x => new
                {
                    x.Id,
                    x.Title,
                    BusinessName = x.Business.Name,
                    CustomerName = x.Customer.Name,
                    x.DueDate,
                    x.PaidDate,
                    x.Total,
                    x.CreatedDate,
                    x.ModifiedDate,
                    x.ProcessedAt,
                    x.ProccessAttempt,
                    x.ReprocessRegularly,
                }),
                nameof(Contracts.Invoicing.Invoice.Id),
                nameof(Contracts.Invoicing.Invoice.Title),
                nameof(Contracts.Invoicing.Invoice.Business) + nameof(Contracts.Invoicing.Invoice.Business.Name),
                nameof(Contracts.Invoicing.Invoice.Customer) + nameof(Contracts.Invoicing.Invoice.Customer.Name),
                nameof(Contracts.Invoicing.Invoice.DueDate),
                nameof(Contracts.Invoicing.Invoice.PaidDate),
                nameof(Contracts.Invoicing.Invoice.Total),
                nameof(Contracts.Invoicing.Invoice.CreatedDate),
                nameof(Contracts.Invoicing.Invoice.ModifiedDate),
                nameof(Contracts.Invoicing.Invoice.ProcessedAt),
                nameof(Contracts.Invoicing.Invoice.ProccessAttempt),
                nameof(Contracts.Invoicing.Invoice.ReprocessRegularly));
        }
    }
}
