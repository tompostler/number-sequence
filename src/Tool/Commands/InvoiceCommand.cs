using System;
using System.Collections.Generic;
using System.CommandLine;
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

            Command businessGetCommand = new("get", "Get and existing invoice business.");
            Argument<long> businessIdArgument = new("id", "The id of the business.");
            businessGetCommand.AddArgument(businessIdArgument);
            businessGetCommand.SetHandler(HandleBusinessGetAsync, businessIdArgument, verbosityOption);

            Command businessListCommand = new("list", "Get and existing invoice business.");
            businessListCommand.SetHandler(HandleBusinessListAsync, verbosityOption);

            businessCommand.AddCommand(businessCreateCommand);
            businessCommand.AddCommand(businessGetCommand);
            businessCommand.AddCommand(businessListCommand);


            Command customerCommand = new("customer", "Manage invoice customers (location that receives invoices).");

            Command customerCreateCommand = new("create", "Create a new invoice customer.");
            customerCreateCommand.SetHandler(HandleCustomerCreateAsync, verbosityOption);

            Command customerGetCommand = new("get", "Get and existing invoice customer.");
            Argument<long> customerIdArgument = new("id", "The id of the customer.");
            customerGetCommand.AddArgument(customerIdArgument);
            customerGetCommand.SetHandler(HandleCustomerGetAsync, customerIdArgument, verbosityOption);

            Command customerListCommand = new("list", "Get and existing invoice customer.");
            customerListCommand.SetHandler(HandleCustomerListAsync, verbosityOption);

            customerCommand.AddCommand(customerCreateCommand);
            customerCommand.AddCommand(customerGetCommand);
            customerCommand.AddCommand(customerListCommand);


            rootCommand.AddCommand(businessCommand);
            rootCommand.AddCommand(customerCommand);
            return rootCommand;
        }

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
    }
}
