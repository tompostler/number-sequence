using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LedgerCommand
    {
        private static async Task HandleCreateAsync(long? fromId, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.Invoice createFromInvoice = default;
            if (fromId.HasValue)
            {
                createFromInvoice = await client.Ledger.GetInvoiceAsync(fromId.Value);
                if (!createFromInvoice.PaidDate.HasValue)
                {
                    throw new InvalidOperationException($"Create from invoice {createFromInvoice.Id} does not have a paid date. Creating invoices from another only supports invoices that are marked paid.");
                }
            }

            Contracts.Ledger.Invoice invoice = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Title = Input.GetString(nameof(invoice.Title)),
                Description = Input.GetString(nameof(invoice.Description)),
                DueDate = Input.GetDateOnly(nameof(invoice.DueDate)),
            };

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
            long businessId = createFromInvoice == default
                ? Input.GetLong("Business.Id", canDefault: false)
                : Input.GetLong("Business.Id", canDefault: true, defaultVal: createFromInvoice.Business.Id);
            invoice.Business = businesses.Single(x => x.Id == businessId);

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
            long customerId = createFromInvoice == default
                ? Input.GetLong("Customer.Id", canDefault: false)
                : Input.GetLong("Customer.Id", canDefault: true, defaultVal: createFromInvoice.Customer.Id);
            invoice.Customer = customers.Single(x => x.Id == customerId);

            invoice = await client.Ledger.CreateInvoiceAsync(invoice);

            if (createFromInvoice != default)
            {
                invoice.Lines ??= new List<Contracts.Ledger.InvoiceLine>();
                invoice.Lines.Add(new()
                {
                    Title = $"Invoice \"{createFromInvoice.Title}\" (id {createFromInvoice.Id:0000}), due {createFromInvoice.DueDate:o}",
                    Description = createFromInvoice.Description,
                    Quantity = 1,
                    Price = createFromInvoice.Total,
                });
                string paymentDescription = createFromInvoice.Payments?.Count > 0
                    ? string.Join(", ", createFromInvoice.Payments.OrderBy(x => x.PaymentDate).Select(x => $"{x.PaymentDate:yyyy-MM-dd} ${x.Amount:N2}"))
                    : null;
                invoice.Lines.Add(new()
                {
                    Title = $"Payment received on {createFromInvoice.PaidDate:o}",
                    Description = paymentDescription,
                    Quantity = 1,
                    Price = -createFromInvoice.Total,
                });
                invoice = await client.Ledger.UpdateInvoiceAsync(invoice);
            }

            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleEditAsync(long id, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.Invoice invoice = await client.Ledger.GetInvoiceAsync(id);
            invoice.Title = Input.GetString(nameof(invoice.Title), invoice.Title);
            invoice.Description = Input.GetString(nameof(invoice.Description), invoice.Description);
            invoice.DueDate = Input.GetDateOnly(nameof(invoice.DueDate), invoice.DueDate);

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
            long businessId = Input.GetLong("Business.Id", defaultVal: invoice.Business.Id);
            invoice.Business = businesses.Single(x => x.Id == businessId);

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
            long customerId = Input.GetLong("Customer.Id", defaultVal: invoice.Customer.Id);
            invoice.Customer = customers.Single(x => x.Id == customerId);

            invoice = await client.Ledger.UpdateInvoiceAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleGetAsync(long id, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Ledger.Invoice invoice = await client.Ledger.GetInvoiceAsync(id);

            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleListAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Contracts.Ledger.Invoice> invoices = await client.Ledger.GetInvoicesAsync();

            Console.WriteLine();
            PrintInvoices(invoices.ToArray());
        }

        private static async Task HandleMarkReprocessRegularlyAsync(long id, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Ledger.Invoice invoice = await client.Ledger.GetInvoiceAsync(id);

            invoice.ReprocessRegularly = true;

            invoice = await client.Ledger.UpdateInvoiceAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleProcessAsync(long id, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Ledger.Invoice invoice = await client.Ledger.GetInvoiceAsync(id);

            invoice.ReadyForProcessing = true;
            invoice.ProcessedAt = default;

            invoice = await client.Ledger.UpdateInvoiceAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }

        private static void PrintSingleInvoice(Contracts.Ledger.Invoice invoice, bool raw)
        {
            if (raw)
            {
                Console.WriteLine(invoice.ToJsonString(indented: true));
            }
            else
            {
                // When displaying an invoice by default, nicely output the data instead of dumping the json out.
                // Primarily for invoices with more lines (as it reduces the amount of scrolling necessary).
                Console.WriteLine();

                Console.WriteLine("Summary:");
                PrintInvoices(invoice);

                Console.WriteLine($"Lines ({invoice.Lines?.Count ?? 0}):");
                Output.WriteTable(
                    invoice.Lines,
                    nameof(Contracts.Ledger.InvoiceLine.Id),
                    nameof(Contracts.Ledger.InvoiceLine.Title),
                    nameof(Contracts.Ledger.InvoiceLine.Description),
                    nameof(Contracts.Ledger.InvoiceLine.Quantity),
                    nameof(Contracts.Ledger.InvoiceLine.Unit),
                    nameof(Contracts.Ledger.InvoiceLine.Price),
                    nameof(Contracts.Ledger.InvoiceLine.Total),
                    nameof(Contracts.Ledger.InvoiceLine.CreatedDate),
                    nameof(Contracts.Ledger.InvoiceLine.ModifiedDate));

                if (invoice.Payments?.Count > 0)
                {
                    Console.WriteLine($"Payments ({invoice.Payments.Count}):");
                    Output.WriteTable(
                        invoice.Payments,
                        nameof(Contracts.Ledger.InvoicePayment.Id),
                        nameof(Contracts.Ledger.InvoicePayment.PaymentDate),
                        nameof(Contracts.Ledger.InvoicePayment.Details),
                        nameof(Contracts.Ledger.InvoicePayment.Amount),
                        nameof(Contracts.Ledger.InvoicePayment.CreatedDate),
                        nameof(Contracts.Ledger.InvoicePayment.ModifiedDate));
                }
            }
        }

        private static void PrintInvoices(params Contracts.Ledger.Invoice[] invoices)
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
                    x.TotalPaid,
                    x.Balance,
                    x.CreatedDate,
                    x.ModifiedDate,
                    x.ProcessedAt,
                    x.ProccessAttempt,
                    x.ReprocessRegularly,
                }),
                nameof(Contracts.Ledger.Invoice.Id),
                nameof(Contracts.Ledger.Invoice.Title),
                nameof(Contracts.Ledger.Invoice.Business) + nameof(Contracts.Ledger.Invoice.Business.Name),
                nameof(Contracts.Ledger.Invoice.Customer) + nameof(Contracts.Ledger.Invoice.Customer.Name),
                nameof(Contracts.Ledger.Invoice.DueDate),
                nameof(Contracts.Ledger.Invoice.PaidDate),
                nameof(Contracts.Ledger.Invoice.Total),
                nameof(Contracts.Ledger.Invoice.TotalPaid),
                nameof(Contracts.Ledger.Invoice.Balance),
                nameof(Contracts.Ledger.Invoice.CreatedDate),
                nameof(Contracts.Ledger.Invoice.ModifiedDate),
                nameof(Contracts.Ledger.Invoice.ProcessedAt),
                nameof(Contracts.Ledger.Invoice.ProccessAttempt),
                nameof(Contracts.Ledger.Invoice.ReprocessRegularly));
        }
    }
}
