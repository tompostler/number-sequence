using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class InvoiceCommand
    {
        private static async Task HandleCreateAsync(long? fromId, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

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

        private static async Task HandleEditAsync(long id, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

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

        private static async Task HandleGetAsync(long id, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(id);

            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleListAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Contracts.Invoicing.Invoice> invoices = await client.Invoice.GetAsync();

            Console.WriteLine();
            PrintInvoices(invoices.ToArray());
        }

        private static async Task HandleMarkPaidAsync(long id, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(id);

            invoice.PaidDate = Input.GetDateOnly(nameof(invoice.PaidDate));
            invoice.PaidDetails = Input.GetString(nameof(invoice.PaidDetails));

            invoice = await client.Invoice.UpdateAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleMarkReprocessRegularlyAsync(long id, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(id);

            invoice.ReprocessRegularly = true;

            invoice = await client.Invoice.UpdateAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleProcessAsync(long id, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
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
                // When displaying an invoice by default, nicely output the data instead of dumping the json out.
                // Primarily for invoices with more lines (as it reduces the amount of scrolling necessary).
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
