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


            rootCommand.AddCommand(businessCommand);
            return rootCommand;
        }

        private static async Task HandleBusinessCreateAsync(Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.InvoiceBusiness invoiceBusiness = new()
            {
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
                nameof(Contracts.Invoicing.InvoiceBusiness.Contact));
        }
    }
}
