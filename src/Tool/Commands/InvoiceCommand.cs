using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Tool.Extensions;

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
            Argument<long> businessIdArgument = new("businessId", "The id of the business.");
            businessGetCommand.AddArgument(businessIdArgument);
            businessGetCommand.SetHandler(HandleBusinessGetAsync, businessIdArgument, verbosityOption);

            Command businessListCommand = new("list", "Get existing invoice businesses.");
            businessListCommand.SetHandler(HandleBusinessListAsync, verbosityOption);

            businessCommand.AddCommand(businessCreateCommand);
            businessCommand.AddCommand(businessGetCommand);
            businessCommand.AddCommand(businessListCommand);
            rootCommand.AddCommand(businessCommand);


            Command customerCommand = new("customer", "Manage invoice customers (location that receives invoices).");

            Command customerCreateCommand = new("create", "Create a new invoice customer.");
            customerCreateCommand.SetHandler(HandleCustomerCreateAsync, verbosityOption);

            Command customerGetCommand = new("get", "Get an existing invoice customer.");
            Argument<long> customerIdArgument = new("customerId", "The id of the customer.");
            customerGetCommand.AddArgument(customerIdArgument);
            customerGetCommand.SetHandler(HandleCustomerGetAsync, customerIdArgument, verbosityOption);

            Command customerListCommand = new("list", "Get existing invoice customers.");
            customerListCommand.SetHandler(HandleCustomerListAsync, verbosityOption);

            customerCommand.AddCommand(customerCreateCommand);
            customerCommand.AddCommand(customerGetCommand);
            customerCommand.AddCommand(customerListCommand);
            rootCommand.AddCommand(customerCommand);


            Command lineDefaultCommand = new("line-default", "Manage invoice line defaults (line items that can be used as a reference for adding a line to an invoice).");

            Command lineDefaultCreateCommand = new("create", "Create a new invoice line default.");
            lineDefaultCreateCommand.SetHandler(HandleLineDefaultCreateAsync, verbosityOption);

            Command lineDefaultGetCommand = new("get", "Get an existing invoice line default.");
            Argument<long> lineDefaultIdArgument = new("lineDefaultId", "The id of the line default.");
            lineDefaultGetCommand.AddArgument(lineDefaultIdArgument);
            lineDefaultGetCommand.SetHandler(HandleLineDefaultGetAsync, lineDefaultIdArgument, verbosityOption);

            Command lineDefaultListCommand = new("list", "Get existing invoice line defaults.");
            lineDefaultListCommand.SetHandler(HandleLineDefaultListAsync, verbosityOption);

            lineDefaultCommand.AddCommand(lineDefaultCreateCommand);
            lineDefaultCommand.AddCommand(lineDefaultGetCommand);
            lineDefaultCommand.AddCommand(lineDefaultListCommand);
            rootCommand.AddCommand(lineDefaultCommand);


            Command lineCommand = new("line", "Manage invoice lines.");

            Command lineCreateCommand = new("create", "Create a new invoice line.");
            Argument<long> idArgument = new("invoiceId", "The id of the invoice.");
            lineCreateCommand.AddArgument(idArgument);
            lineCreateCommand.SetHandler(HandleLineCreateAsync, idArgument, verbosityOption);

            Command lineEditCommand = new("edit", "Edit an existing invoice line.");
            Argument<long> lineIdArgument = new("lineId", "The id of the line.");
            lineEditCommand.AddArgument(idArgument);
            lineEditCommand.AddArgument(lineIdArgument);
            lineEditCommand.SetHandler(HandleLineEditAsync, idArgument, lineIdArgument, verbosityOption);

            Command lineGetCommand = new("get", "Get an existing invoice line.");
            lineGetCommand.AddArgument(idArgument);
            lineGetCommand.AddArgument(lineIdArgument);
            lineGetCommand.SetHandler(HandleLineGetAsync, idArgument, lineIdArgument, verbosityOption);

            Command lineListCommand = new("list", "Get existing invoice lines.");
            lineListCommand.AddArgument(idArgument);
            lineListCommand.SetHandler(HandleLineListAsync, idArgument, verbosityOption);

            Command lineRemoveCommand = new("remove", "Remove an existing invoice line.");
            lineRemoveCommand.AddArgument(idArgument);
            lineRemoveCommand.AddArgument(lineIdArgument);
            lineRemoveCommand.SetHandler(HandleLineRemoveAsync, idArgument, lineIdArgument, verbosityOption);

            lineCommand.AddCommand(lineCreateCommand);
            lineCommand.AddCommand(lineEditCommand);
            lineCommand.AddCommand(lineGetCommand);
            lineCommand.AddCommand(lineListCommand);
            lineCommand.AddCommand(lineRemoveCommand);
            rootCommand.AddCommand(lineCommand);


            Command createCommand = new("create", "Create a new invoice.");
            createCommand.SetHandler(HandleCreateAsync, verbosityOption);
            rootCommand.AddCommand(createCommand);


            Command editCommand = new("edit", "Edit an existing invoice.");
            editCommand.AddArgument(idArgument);
            editCommand.SetHandler(HandleEditAsync, idArgument, verbosityOption);
            rootCommand.AddCommand(editCommand);


            Command getCommand = new("get", "Get an existing invoice.");
            getCommand.AddArgument(idArgument);
            getCommand.SetHandler(HandleGetAsync, idArgument, verbosityOption);
            rootCommand.AddCommand(getCommand);


            Command listCommand = new("list", "Get existing invoices.");
            listCommand.SetHandler(HandleListAsync, verbosityOption);
            rootCommand.AddCommand(listCommand);


            Command markPaidCommand = new("mark-paid", "Mark a specific invoice as paid.");
            markPaidCommand.AddArgument(idArgument);
            markPaidCommand.SetHandler(HandleMarkPaidAsync, idArgument, verbosityOption);
            rootCommand.AddCommand(markPaidCommand);


            Command processCommand = new("process", "Mark a specific invoice for [re-]processing to pdf.");
            processCommand.AddAlias("reprocess");
            processCommand.AddArgument(idArgument);
            processCommand.SetHandler(HandleProcessAsync, idArgument, verbosityOption);
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
            Console.WriteLine(invoiceBusiness.ToJsonString());
        }

        private static async Task HandleBusinessGetAsync(long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.InvoiceBusiness invoiceBusiness = await client.Invoice.GetBusinessAsync(id);
            Console.WriteLine(invoiceBusiness.ToJsonString());
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
            Console.WriteLine(invoiceCustomer.ToJsonString());
        }

        private static async Task HandleCustomerGetAsync(long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.InvoiceCustomer invoiceCustomer = await client.Invoice.GetCustomerAsync(id);
            Console.WriteLine(invoiceCustomer.ToJsonString());
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
            Console.WriteLine(invoiceLineDefault.ToJsonString());
        }

        private static async Task HandleLineDefaultGetAsync(long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.InvoiceLineDefault invoiceLineDefault = await client.Invoice.GetLineDefaultAsync(id);
            Console.WriteLine(invoiceLineDefault.ToJsonString());
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
                nameof(Contracts.Invoicing.InvoiceLineDefault.CreatedDate));
        }

        #endregion // Line Defaults

        #region Lines

        private static async Task HandleLineCreateAsync(long invoiceId, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(invoiceId);
            PrintInvoices(invoice);

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
                nameof(Contracts.Invoicing.InvoiceLineDefault.CreatedDate));

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
            Console.WriteLine(invoice.ToJsonString());
        }

        private static async Task HandleLineEditAsync(long invoiceId, long id, Verbosity verbosity)
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
            Console.WriteLine(invoice.ToJsonString());
        }

        private static async Task HandleLineGetAsync(long invoiceId, long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(invoiceId);
            Contracts.Invoicing.InvoiceLine invoiceLine = invoice.Lines.Single(x => x.Id == id);
            Console.WriteLine(invoiceLine.ToJsonString());
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
                nameof(Contracts.Invoicing.InvoiceLine.Quantity),
                nameof(Contracts.Invoicing.InvoiceLine.Unit),
                nameof(Contracts.Invoicing.InvoiceLine.Price),
                nameof(Contracts.Invoicing.InvoiceLine.Total),
                nameof(Contracts.Invoicing.InvoiceLine.CreatedDate),
                nameof(Contracts.Invoicing.InvoiceLine.ModifiedDate));
        }

        private static async Task HandleLineRemoveAsync(long invoiceId, long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(invoiceId);
            Contracts.Invoicing.InvoiceLine invoiceLine = invoice.Lines.Single(x => x.Id == id);
            _ = invoice.Lines.Remove(invoiceLine);
            Console.WriteLine(invoiceLine.ToJsonString());

            invoice = await client.Invoice.UpdateAsync(invoice);
            Console.WriteLine(invoice.ToJsonString());
        }

        #endregion // Lines

        private static async Task HandleCreateAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.Invoice invoice = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Title = Input.GetString(nameof(invoice.Title)),
                Description = Input.GetString(nameof(invoice.Description)),
                DueDate = Input.GetDateTime(nameof(invoice.DueDate)),
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
            long invoiceBusinessId = Input.GetLong("Business.Id", canDefault: false);
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
            long invoiceCustomerId = Input.GetLong("Customer.Id", canDefault: false);
            invoice.Customer = invoiceCustomers.Single(x => x.Id == invoiceCustomerId);

            invoice = await client.Invoice.CreateAsync(invoice);
            Console.WriteLine(invoice.ToJsonString());
        }

        private static async Task HandleEditAsync(long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(id);
            invoice.Title = Input.GetString(nameof(invoice.Title), invoice.Title);
            invoice.Description = Input.GetString(nameof(invoice.Description), invoice.Description);
            invoice.DueDate = Input.GetDateTime(nameof(invoice.DueDate), invoice.DueDate);

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
            Console.WriteLine(invoice.ToJsonString());
        }

        private static async Task HandleGetAsync(long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(id);
            Console.WriteLine(invoice.ToJsonString());
        }

        private static async Task HandleListAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            List<Contracts.Invoicing.Invoice> invoices = await client.Invoice.GetAsync();

            Console.WriteLine();
            PrintInvoices(invoices.ToArray());
        }

        private static async Task HandleMarkPaidAsync(long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(id);

            invoice.PaidDate = Input.GetDateTime(nameof(invoice.PaidDate));
            invoice.PaidDetails = Input.GetString(nameof(invoice.PaidDetails));

            invoice = await client.Invoice.UpdateAsync(invoice);
            Console.WriteLine(invoice.ToJsonString());
        }

        private static async Task HandleProcessAsync(long id, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);
            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(id);

            invoice.ReadyForProcessing = true;
            invoice.ProcessedAt = default;

            invoice = await client.Invoice.UpdateAsync(invoice);
            Console.WriteLine(invoice.ToJsonString());
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
                nameof(Contracts.Invoicing.Invoice.ProccessAttempt)
                );
        }
    }
}
