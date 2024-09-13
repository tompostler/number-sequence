using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class InvoiceCommand
    {
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

        private static async Task HandleLineSwapAsync(long invoiceId, long id, long otherId, bool raw, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, Program.Stamp);

            Contracts.Invoicing.Invoice invoice = await client.Invoice.GetAsync(invoiceId);
            Contracts.Invoicing.InvoiceLine invoiceLine1 = invoice.Lines.Single(x => x.Id == id);
            Contracts.Invoicing.InvoiceLine invoiceLine2 = invoice.Lines.Single(x => x.Id == otherId);

            (invoiceLine1.Title, invoiceLine2.Title) = (invoiceLine2.Title, invoiceLine1.Title);
            (invoiceLine1.Description, invoiceLine2.Description) = (invoiceLine2.Description, invoiceLine1.Description);
            (invoiceLine1.Quantity, invoiceLine2.Quantity) = (invoiceLine2.Quantity, invoiceLine1.Quantity);
            (invoiceLine1.Unit, invoiceLine2.Unit) = (invoiceLine2.Unit, invoiceLine1.Unit);
            (invoiceLine1.Price, invoiceLine2.Price) = (invoiceLine2.Price, invoiceLine1.Price);

            invoice = await client.Invoice.UpdateAsync(invoice);
            PrintSingleInvoice(invoice, raw);
        }
    }
}
