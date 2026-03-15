using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LedgerCommand
    {
        private static async Task HandleStatementCreateAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            List<Contracts.Ledger.Business> businesses = await client.Ledger.GetBusinessesAsync();
            Console.WriteLine("Invoice businesses:");
            Output.WriteTable(
                businesses,
                nameof(Contracts.Ledger.Business.Id),
                nameof(Contracts.Ledger.Business.Name),
                nameof(Contracts.Ledger.Business.AddressLine1),
                nameof(Contracts.Ledger.Business.AddressLine2),
                nameof(Contracts.Ledger.Business.Contact),
                nameof(Contracts.Ledger.Business.CreatedDate));
            long businessId = Input.GetLong("Business.Id", canDefault: false);

            List<Contracts.Ledger.Customer> customers = await client.Ledger.GetCustomersAsync();
            Console.WriteLine("Invoice customers:");
            Output.WriteTable(
                customers,
                nameof(Contracts.Ledger.Customer.Id),
                nameof(Contracts.Ledger.Customer.Name),
                nameof(Contracts.Ledger.Customer.AddressLine1),
                nameof(Contracts.Ledger.Customer.AddressLine2),
                nameof(Contracts.Ledger.Customer.Contact),
                nameof(Contracts.Ledger.Customer.CreatedDate));
            long customerId = Input.GetLong("Customer.Id", canDefault: false);

            Contracts.Ledger.Statement statement = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Business = businesses.Single(x => x.Id == businessId),
                Customer = customers.Single(x => x.Id == customerId),
                InvoiceStartDate = Input.GetDateOnly(nameof(statement.InvoiceStartDate), new DateOnly(DateTime.Now.Year, 1, 1)),
                InvoiceEndDate = Input.GetDateOnly(nameof(statement.InvoiceEndDate), new DateOnly(DateTime.Now.Year + 1, 1, 1)),
                SearchByDueDate = Input.GetBool(nameof(statement.SearchByDueDate), defaultVal: true),
                ReadyForProcessing = Input.GetBool(nameof(statement.ReadyForProcessing), defaultVal: true),
            };

            statement = await client.Ledger.CreateStatementAsync(statement);
            PrintSingleStatement(statement);
        }

        private static async Task HandleStatementGetAsync(long id, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Ledger.Statement statement = await client.Ledger.GetStatementAsync(id);

            PrintSingleStatement(statement);
        }

        private static async Task HandleStatementListAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Contracts.Ledger.Statement> statements = await client.Ledger.GetStatementsAsync();

            Console.WriteLine();
            PrintStatements(statements.ToArray());
        }

        private static async Task HandleStatementProcessAsync(long id, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.Statement statement = await client.Ledger.UpdateStatementForProcessAsync(id);
            PrintSingleStatement(statement);
        }

        private static void PrintSingleStatement(Contracts.Ledger.Statement statement)
        {
            Console.WriteLine();

            Console.WriteLine("Summary:");
            PrintStatements(statement);

            Console.WriteLine($"Invoices ({statement.Invoices?.Count ?? 0}):");
            if (statement.Invoices?.Count > 0)
            {
                PrintInvoices(statement.Invoices.ToArray());
            }

            List<Contracts.Ledger.InvoicePayment> allPayments = statement.Invoices?
                .Where(x => x.Payments?.Count > 0)
                .SelectMany(x => x.Payments)
                .ToList() ?? [];
            Console.WriteLine($"Payments ({allPayments.Count}):");
            if (allPayments.Count > 0)
            {
                Output.WriteTable(
                    allPayments.Select(x => new
                    {
                        InvoiceId = x.Invoice?.Id,
                        x.Id,
                        x.PaymentDate,
                        x.Details,
                        x.Amount,
                        x.CreatedDate,
                        x.ModifiedDate,
                    }),
                    "InvoiceId",
                    nameof(Contracts.Ledger.InvoicePayment.Id),
                    nameof(Contracts.Ledger.InvoicePayment.PaymentDate),
                    nameof(Contracts.Ledger.InvoicePayment.Details),
                    nameof(Contracts.Ledger.InvoicePayment.Amount),
                    nameof(Contracts.Ledger.InvoicePayment.CreatedDate),
                    nameof(Contracts.Ledger.InvoicePayment.ModifiedDate));
            }
        }

        private static void PrintStatements(params Contracts.Ledger.Statement[] statements)
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
                nameof(Contracts.Ledger.Statement.Id),
                nameof(Contracts.Ledger.Statement.Business) + nameof(Contracts.Ledger.Statement.Business.Name),
                nameof(Contracts.Ledger.Statement.Customer) + nameof(Contracts.Ledger.Statement.Customer.Name),
                nameof(Contracts.Ledger.Statement.InvoiceStartDate),
                nameof(Contracts.Ledger.Statement.InvoiceEndDate),
                nameof(Contracts.Ledger.Statement.TotalBilled),
                nameof(Contracts.Ledger.Statement.TotalPaid),
                nameof(Contracts.Ledger.Statement.CreatedDate),
                nameof(Contracts.Ledger.Statement.ModifiedDate),
                nameof(Contracts.Ledger.Statement.ProcessedAt),
                nameof(Contracts.Ledger.Statement.ProccessAttempt));
        }
    }
}
