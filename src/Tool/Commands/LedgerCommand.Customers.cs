using TcpWtf.NumberSequence.Client;
using Unlimitedinf.Utilities;
using Unlimitedinf.Utilities.Extensions;

namespace TcpWtf.NumberSequence.Tool.Commands
{
    internal static partial class LedgerCommand
    {
        private static async Task HandleCustomerCreateAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.Customer customer = new()
            {
                AccountName = TokenProvider.GetAccount(),
                Name = Input.GetString(nameof(customer.Name)),
                AddressLine1 = Input.GetString(nameof(customer.AddressLine1)),
                AddressLine2 = Input.GetString(nameof(customer.AddressLine2)),
                Contact = Input.GetString(nameof(customer.Contact)),
            };

            customer = await client.Ledger.CreateCustomerAsync(customer);
            Console.WriteLine(customer.ToJsonString(indented: true));
        }

        private static async Task HandleCustomerEditAsync(long id, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);

            Contracts.Ledger.Customer customer = await client.Ledger.GetCustomerAsync(id);

            customer.Name = Input.GetString(nameof(customer.Name), customer.Name);
            customer.AddressLine1 = Input.GetString(nameof(customer.AddressLine1), customer.AddressLine1);
            customer.AddressLine2 = Input.GetString(nameof(customer.AddressLine2), customer.AddressLine2);
            customer.Contact = Input.GetString(nameof(customer.Contact), customer.Contact);

            customer = await client.Ledger.UpdateCustomerAsync(customer);
            Console.WriteLine(customer.ToJsonString(indented: true));
        }

        private static async Task HandleCustomerGetAsync(long id, Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            Contracts.Ledger.Customer customer = await client.Ledger.GetCustomerAsync(id);
            Console.WriteLine(customer.ToJsonString(indented: true));
        }

        private static async Task HandleCustomerListAsync(Stamp stamp, Verbosity verbosity)
        {
            NsTcpWtfClient client = new(new Logger<NsTcpWtfClient>(verbosity), TokenProvider.GetAsync, stamp);
            List<Contracts.Ledger.Customer> customers = await client.Ledger.GetCustomersAsync();

            Console.WriteLine();
            Output.WriteTable(
                customers,
                nameof(Contracts.Ledger.Customer.Id),
                nameof(Contracts.Ledger.Customer.Name),
                nameof(Contracts.Ledger.Customer.AddressLine1),
                nameof(Contracts.Ledger.Customer.AddressLine2),
                nameof(Contracts.Ledger.Customer.Contact),
                nameof(Contracts.Ledger.Customer.CreatedDate));
        }
    }
}
