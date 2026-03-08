using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LedgerCommand
    {
        private static async Task HandleLineDefaultCreateAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.InvoiceLineDefault invoiceLineDefault = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Title = Input.GetString(nameof(invoiceLineDefault.Title)),
                Description = Input.GetString(nameof(invoiceLineDefault.Description)),
                Quantity = Input.GetDecimal(nameof(invoiceLineDefault.Quantity), defaultVal: 1),
                Unit = Input.GetString(nameof(invoiceLineDefault.Unit)),
                Price = Input.GetDecimal(nameof(invoiceLineDefault.Price)),
            };

            invoiceLineDefault = await client.Ledger.CreateInvoiceLineDefaultAsync(invoiceLineDefault);
            Console.WriteLine(invoiceLineDefault.ToJsonString(indented: true));
        }

        private static async Task HandleLineDefaultEditAsync(long id, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.InvoiceLineDefault invoiceLineDefault = await client.Ledger.GetInvoiceLineDefaultAsync(id);

            invoiceLineDefault.Title = Input.GetString(nameof(invoiceLineDefault.Title), invoiceLineDefault.Title);
            invoiceLineDefault.Description = Input.GetString(nameof(invoiceLineDefault.Description), invoiceLineDefault.Description);
            invoiceLineDefault.Quantity = Input.GetDecimal(nameof(invoiceLineDefault.Quantity), defaultVal: invoiceLineDefault.Quantity);
            invoiceLineDefault.Unit = Input.GetString(nameof(invoiceLineDefault.Unit), invoiceLineDefault.Unit);
            invoiceLineDefault.Price = Input.GetDecimal(nameof(invoiceLineDefault.Price), defaultVal: invoiceLineDefault.Price);

            invoiceLineDefault = await client.Ledger.UpdateInvoiceLineDefaultAsync(invoiceLineDefault);
            Console.WriteLine(invoiceLineDefault.ToJsonString(indented: true));
        }

        private static async Task HandleLineDefaultGetAsync(long id, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Ledger.InvoiceLineDefault invoiceLineDefault = await client.Ledger.GetInvoiceLineDefaultAsync(id);
            Console.WriteLine(invoiceLineDefault.ToJsonString(indented: true));
        }

        private static async Task HandleLineDefaultListAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Contracts.Ledger.InvoiceLineDefault> invoiceLineDefaults = await client.Ledger.GetInvoiceLineDefaultsAsync();

            Console.WriteLine();
            Output.WriteTable(
                invoiceLineDefaults,
                nameof(Contracts.Ledger.InvoiceLineDefault.Id),
                nameof(Contracts.Ledger.InvoiceLineDefault.Title),
                nameof(Contracts.Ledger.InvoiceLineDefault.Description),
                nameof(Contracts.Ledger.InvoiceLineDefault.Quantity),
                nameof(Contracts.Ledger.InvoiceLineDefault.Unit),
                nameof(Contracts.Ledger.InvoiceLineDefault.Price),
                nameof(Contracts.Ledger.InvoiceLineDefault.ModifiedDate));
        }

        private static async Task HandleLineDefaultSwapAsync(long id, long otherId, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.InvoiceLineDefault invoiceLineDefault1 = await client.Ledger.GetInvoiceLineDefaultAsync(id);
            Contracts.Ledger.InvoiceLineDefault invoiceLineDefault2 = await client.Ledger.GetInvoiceLineDefaultAsync(otherId);

            (invoiceLineDefault1.Title, invoiceLineDefault2.Title) = (invoiceLineDefault2.Title, invoiceLineDefault1.Title);
            (invoiceLineDefault1.Description, invoiceLineDefault2.Description) = (invoiceLineDefault2.Description, invoiceLineDefault1.Description);
            (invoiceLineDefault1.Quantity, invoiceLineDefault2.Quantity) = (invoiceLineDefault2.Quantity, invoiceLineDefault1.Quantity);
            (invoiceLineDefault1.Unit, invoiceLineDefault2.Unit) = (invoiceLineDefault2.Unit, invoiceLineDefault1.Unit);
            (invoiceLineDefault1.Price, invoiceLineDefault2.Price) = (invoiceLineDefault2.Price, invoiceLineDefault1.Price);

            invoiceLineDefault1 = await client.Ledger.UpdateInvoiceLineDefaultAsync(invoiceLineDefault1);
            invoiceLineDefault2 = await client.Ledger.UpdateInvoiceLineDefaultAsync(invoiceLineDefault2);
            Console.WriteLine(invoiceLineDefault1.ToJsonString(indented: true));
            Console.WriteLine(invoiceLineDefault2.ToJsonString(indented: true));
        }
    }
}
