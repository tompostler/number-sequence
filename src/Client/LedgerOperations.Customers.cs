using TcpWtf.NumberSequence.Contracts.Ledger;

namespace TcpWtf.NumberSequence.Client
{
    public sealed partial class LedgerOperations
    {
        /// <summary>
        /// Create a new customer.
        /// </summary>
        public async Task<Customer> CreateCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "ledger/customers")
                {
                    Content = customer.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Customer>(cancellationToken);
        }

        /// <summary>
        /// Get an existing customer.
        /// </summary>
        public async Task<Customer> GetCustomerAsync(long id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"ledger/customers/{id}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Customer>(cancellationToken);
        }

        /// <summary>
        /// Get existing customers.
        /// </summary>
        public async Task<List<Customer>> GetCustomersAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "ledger/customers"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<Customer>>(cancellationToken);
        }

        /// <summary>
        /// Update an existing customer.
        /// </summary>
        public async Task<Customer> UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    "ledger/customers")
                {
                    Content = customer.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Customer>(cancellationToken);
        }
    }
}
