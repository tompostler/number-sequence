using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LedgerCommand
    {
        private static async Task HandleLineCreateAsync(long invoiceId, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.Invoice invoice = await client.Ledger.GetInvoiceAsync(invoiceId);
            PrintSingleInvoice(invoice, raw);

            List<Contracts.Ledger.InvoiceLineDefault> invoiceLineDefaults = await client.Ledger.GetInvoiceLineDefaultsAsync();
            invoiceLineDefaults.Insert(0, new() { Id = 0, Title = "Non-default entry" });
            Console.WriteLine("Invoice line defaults:");
            Output.WriteTable(
                invoiceLineDefaults,
                nameof(Contracts.Ledger.InvoiceLineDefault.Id),
                nameof(Contracts.Ledger.InvoiceLineDefault.Title),
                nameof(Contracts.Ledger.InvoiceLineDefault.Description),
                nameof(Contracts.Ledger.InvoiceLineDefault.Quantity),
                nameof(Contracts.Ledger.InvoiceLineDefault.Unit),
                nameof(Contracts.Ledger.InvoiceLineDefault.Price),
                nameof(Contracts.Ledger.InvoiceLineDefault.ModifiedDate));

            long invoiceLineDefaultId = Input.GetLong("InvoiceLineDefault.Id");
            Contracts.Ledger.InvoiceLineDefault invoiceLineDefault = invoiceLineDefaults.Skip(1).SingleOrDefault(x => x.Id == invoiceLineDefaultId);
            Contracts.Ledger.InvoiceLine invoiceLine;
            invoiceLine = new()
            {
                Title = Input.GetString(nameof(invoiceLine.Title), invoiceLineDefault?.Title),
                Description = Input.GetString(nameof(invoiceLine.Description), invoiceLineDefault?.Description),
                Quantity = Input.GetDecimal(nameof(invoiceLine.Quantity), defaultVal: invoiceLineDefault?.Quantity ?? 1),
                Unit = Input.GetString(nameof(invoiceLine.Unit), invoiceLineDefault?.Unit),
                Price = Input.GetDecimal(nameof(invoiceLine.Price), defaultVal: invoiceLineDefault?.Price ?? 0),
            };

            invoice.Lines.Add(invoiceLine);
            invoice = await client.Ledger.UpdateInvoiceAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleLineDuplicateAsync(long invoiceId, long id, long targetInvoiceId, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.Invoice sourceInvoice = await client.Ledger.GetInvoiceAsync(invoiceId);
            Contracts.Ledger.InvoiceLine sourceInvoiceLine = sourceInvoice.Lines.Single(x => x.Id == id);

            Contracts.Ledger.Invoice targetInvoice = targetInvoiceId == -1 ? sourceInvoice : await client.Ledger.GetInvoiceAsync(targetInvoiceId);
            Contracts.Ledger.InvoiceLine targetInvoiceLine;
            targetInvoiceLine = new()
            {
                Title = Input.GetString(nameof(sourceInvoiceLine.Title), sourceInvoiceLine.Title),
                Description = Input.GetString(nameof(sourceInvoiceLine.Description), sourceInvoiceLine.Description),
                Quantity = Input.GetDecimal(nameof(sourceInvoiceLine.Quantity), defaultVal: sourceInvoiceLine.Quantity),
                Unit = Input.GetString(nameof(sourceInvoiceLine.Unit), sourceInvoiceLine.Unit),
                Price = Input.GetDecimal(nameof(sourceInvoiceLine.Price), defaultVal: sourceInvoiceLine.Price),
            };

            targetInvoice.Lines.Add(targetInvoiceLine);
            targetInvoice = await client.Ledger.UpdateInvoiceAsync(targetInvoice);
            PrintSingleInvoice(targetInvoice, raw);
        }

        private static async Task HandleLineEditAsync(long invoiceId, long id, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.Invoice invoice = await client.Ledger.GetInvoiceAsync(invoiceId);
            Contracts.Ledger.InvoiceLine invoiceLine = invoice.Lines.Single(x => x.Id == id);

            invoiceLine.Title = Input.GetString(nameof(invoiceLine.Title), invoiceLine.Title);
            invoiceLine.Description = Input.GetString(nameof(invoiceLine.Description), invoiceLine.Description);
            invoiceLine.Quantity = Input.GetDecimal(nameof(invoiceLine.Quantity), defaultVal: invoiceLine.Quantity);
            invoiceLine.Unit = Input.GetString(nameof(invoiceLine.Unit), invoiceLine.Unit);
            invoiceLine.Price = Input.GetDecimal(nameof(invoiceLine.Price), defaultVal: invoiceLine.Price);

            invoice = await client.Ledger.UpdateInvoiceAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleLineGetAsync(long invoiceId, long id, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Ledger.Invoice invoice = await client.Ledger.GetInvoiceAsync(invoiceId);
            Contracts.Ledger.InvoiceLine invoiceLine = invoice.Lines.Single(x => x.Id == id);
            Console.WriteLine(invoiceLine.ToJsonString(indented: true));
        }

        private static async Task HandleLineListAsync(long invoiceId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.Invoice invoice = await client.Ledger.GetInvoiceAsync(invoiceId);

            Console.WriteLine();
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
        }

        private static async Task HandleLineRemoveAsync(long invoiceId, long id, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.Invoice invoice = await client.Ledger.GetInvoiceAsync(invoiceId);
            Contracts.Ledger.InvoiceLine invoiceLine = invoice.Lines.Single(x => x.Id == id);
            _ = invoice.Lines.Remove(invoiceLine);
            Console.WriteLine(invoiceLine.ToJsonString(indented: true));

            invoice = await client.Ledger.UpdateInvoiceAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }

        private static async Task HandleLineSwapAsync(long invoiceId, long id, long otherId, bool raw, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.Invoice invoice = await client.Ledger.GetInvoiceAsync(invoiceId);
            Contracts.Ledger.InvoiceLine invoiceLine1 = invoice.Lines.Single(x => x.Id == id);
            Contracts.Ledger.InvoiceLine invoiceLine2 = invoice.Lines.Single(x => x.Id == otherId);

            (invoiceLine1.Title, invoiceLine2.Title) = (invoiceLine2.Title, invoiceLine1.Title);
            (invoiceLine1.Description, invoiceLine2.Description) = (invoiceLine2.Description, invoiceLine1.Description);
            (invoiceLine1.Quantity, invoiceLine2.Quantity) = (invoiceLine2.Quantity, invoiceLine1.Quantity);
            (invoiceLine1.Unit, invoiceLine2.Unit) = (invoiceLine2.Unit, invoiceLine1.Unit);
            (invoiceLine1.Price, invoiceLine2.Price) = (invoiceLine2.Price, invoiceLine1.Price);

            invoice = await client.Ledger.UpdateInvoiceAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }
    }
}
