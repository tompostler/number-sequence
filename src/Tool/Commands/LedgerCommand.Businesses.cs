using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LedgerCommand
    {
        private static async Task HandleBusinessCreateAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.Business business = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Name = Input.GetString(nameof(business.Name)),
                PayableName = Input.GetString(nameof(business.PayableName)),
                AddressLine1 = Input.GetString(nameof(business.AddressLine1)),
                AddressLine2 = Input.GetString(nameof(business.AddressLine2)),
                Contact = Input.GetString(nameof(business.Contact)),
            };

            business = await client.Ledger.CreateBusinessAsync(business);
            Console.WriteLine(business.ToJsonString(indented: true));
        }

        private static async Task HandleBusinessGetAsync(long id, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Ledger.Business business = await client.Ledger.GetBusinessAsync(id);
            Console.WriteLine(business.ToJsonString(indented: true));
        }

        private static async Task HandleBusinessListAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Contracts.Ledger.Business> businesses = await client.Ledger.GetBusinessesAsync();

            Console.WriteLine();
            Output.WriteTable(
                businesses,
                nameof(Contracts.Ledger.Business.Id),
                nameof(Contracts.Ledger.Business.Name),
                nameof(Contracts.Ledger.Business.PayableName),
                nameof(Contracts.Ledger.Business.AddressLine1),
                nameof(Contracts.Ledger.Business.AddressLine2),
                nameof(Contracts.Ledger.Business.Contact),
                nameof(Contracts.Ledger.Business.CreatedDate));
        }
    }
}
