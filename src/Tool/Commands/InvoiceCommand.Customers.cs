using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class InvoiceCommand
    {
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
    }
}
