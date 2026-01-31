using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class InvoiceCommand
    {
        private static async Task HandleStatementCreateAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

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

            Contracts.Invoicing.Statement statement = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Business = invoiceBusinesses.Single(x => x.Id == invoiceBusinessId),
                Customer = invoiceCustomers.Single(x => x.Id == invoiceCustomerId),
                InvoiceStartDate = Input.GetDateOnly(nameof(statement.InvoiceStartDate)),
                InvoiceEndDate = Input.GetDateOnly(nameof(statement.InvoiceEndDate)),
                ReadyForProcessing = Input.GetBool(nameof(statement.ReadyForProcessing), defaultVal: true),
            };

            statement = await client.Invoice.CreateStatementAsync(statement);
            PrintSingleStatement(statement);
        }

        private static async Task HandleStatementGetAsync(long id, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Invoicing.Statement statement = await client.Invoice.GetStatementAsync(id);

            PrintSingleStatement(statement);
        }

        private static async Task HandleStatementListAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Contracts.Invoicing.Statement> statements = await client.Invoice.GetStatementsAsync();

            Console.WriteLine();
            PrintStatements(statements.ToArray());
        }

        private static async Task HandleStatementProcessAsync(long id, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Invoicing.Statement statement = await client.Invoice.UpdateStatementForProcessAsync(id);
            PrintSingleStatement(statement);
        }

        private static void PrintSingleStatement(Contracts.Invoicing.Statement statement)
        {
            Console.WriteLine();

            Console.WriteLine("Summary:");
            PrintStatements(statement);

            Console.WriteLine($"Invoices ({statement.Invoices?.Count ?? 0}):");
            if (statement.Invoices?.Count > 0)
            {
                PrintInvoices(statement.Invoices.ToArray());
            }
        }

        private static void PrintStatements(params Contracts.Invoicing.Statement[] statements)
        {
            Output.WriteTable(
                statements.Select(x => new
                {
                    x.Id,
                    BusinessName = x.Business.Name,
                    CustomerName = x.Customer.Name,
                    x.InvoiceStartDate,
                    x.InvoiceEndDate,
                    x.TotalBilled,
                    x.TotalPaid,
                    x.CreatedDate,
                    x.ModifiedDate,
                    x.ProcessedAt,
                    x.ProccessAttempt,
                }),
                nameof(Contracts.Invoicing.Statement.Id),
                nameof(Contracts.Invoicing.Statement.Business) + nameof(Contracts.Invoicing.Statement.Business.Name),
                nameof(Contracts.Invoicing.Statement.Customer) + nameof(Contracts.Invoicing.Statement.Customer.Name),
                nameof(Contracts.Invoicing.Statement.InvoiceStartDate),
                nameof(Contracts.Invoicing.Statement.InvoiceEndDate),
                nameof(Contracts.Invoicing.Statement.TotalBilled),
                nameof(Contracts.Invoicing.Statement.TotalPaid),
                nameof(Contracts.Invoicing.Statement.CreatedDate),
                nameof(Contracts.Invoicing.Statement.ModifiedDate),
                nameof(Contracts.Invoicing.Statement.ProcessedAt),
                nameof(Contracts.Invoicing.Statement.ProccessAttempt));
        }
    }
}
