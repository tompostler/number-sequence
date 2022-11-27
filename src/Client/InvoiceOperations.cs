using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts.Invoicing;

namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Invoice operations.
    /// </summary>
    public sealed class InvoiceOperations
    {
        private readonly NsTcpWtfClient nsTcpWtfClient;

        internal InvoiceOperations(NsTcpWtfClient nsTcpWtfClient)
        {
            this.nsTcpWtfClient = nsTcpWtfClient;
        }

        /// <summary>
        /// Create a new business.
        /// </summary>
        public async Task<InvoiceBusiness> CreateBusinessAsync(InvoiceBusiness business, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "invoices/businesses")
                {
                    Content = business.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<InvoiceBusiness>(cancellationToken);
        }

        /// <summary>
        /// Create a new customer.
        /// </summary>
        public async Task<InvoiceCustomer> CreateCustomerAsync(InvoiceCustomer customer, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "invoices/customers")
                {
                    Content = customer.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<InvoiceCustomer>(cancellationToken);
        }

        /// <summary>
        /// Create a new invoice.
        /// </summary>
        public async Task<Invoice> CreateAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "invoices")
                {
                    Content = invoice.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Invoice>(cancellationToken);
        }

        /// <summary>
        /// Create a new line default.
        /// </summary>
        public async Task<InvoiceLineDefault> CreateLineDefaultAsync(InvoiceLineDefault lineDefault, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Post,
                    "invoices/linedefaults")
                {
                    Content = lineDefault.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<InvoiceLineDefault>(cancellationToken);
        }

        /// <summary>
        /// Get an existing business.
        /// </summary>
        public async Task<InvoiceBusiness> GetBusinessAsync(long id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"invoices/businesses/{id}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<InvoiceBusiness>(cancellationToken);
        }

        /// <summary>
        /// Get existing businesses.
        /// </summary>
        public async Task<List<InvoiceBusiness>> GetBusinessesAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "invoices/businesses"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<InvoiceBusiness>>(cancellationToken);
        }

        /// <summary>
        /// Get an existing customer.
        /// </summary>
        public async Task<InvoiceCustomer> GetCustomerAsync(long id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"invoices/customers/{id}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<InvoiceCustomer>(cancellationToken);
        }

        /// <summary>
        /// Get existing customers.
        /// </summary>
        public async Task<List<InvoiceCustomer>> GetCustomersAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "invoices/customers"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<InvoiceCustomer>>(cancellationToken);
        }

        /// <summary>
        /// Get an existing invoice.
        /// </summary>
        public async Task<Invoice> GetAsync(long id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"invoices/{id}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Invoice>(cancellationToken);
        }

        /// <summary>
        /// Get existing invoices.
        /// </summary>
        public async Task<List<Invoice>> GetAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"invoices?skip={skip}&take={take}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<Invoice>>(cancellationToken);
        }

        /// <summary>
        /// Get an existing line default.
        /// </summary>
        public async Task<InvoiceLineDefault> GetLineDefaultAsync(long id, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    $"invoices/linedefaults/{id}"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<InvoiceLineDefault>(cancellationToken);
        }

        /// <summary>
        /// Get existing line defaults.
        /// </summary>
        public async Task<List<InvoiceLineDefault>> GetLineDefaultsAsync(CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Get,
                    "invoices/linedefaults"),
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<List<InvoiceLineDefault>>(cancellationToken);
        }

        /// <summary>
        /// Update an existing business.
        /// </summary>
        public async Task<InvoiceBusiness> UpdateBusinessAsync(InvoiceBusiness business, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    "invoices/businesses")
                {
                    Content = business.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<InvoiceBusiness>(cancellationToken);
        }

        /// <summary>
        /// Update an existing customer.
        /// </summary>
        public async Task<InvoiceCustomer> UpdateCustomerAsync(InvoiceCustomer customer, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    "invoices/customers")
                {
                    Content = customer.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<InvoiceCustomer>(cancellationToken);
        }

        /// <summary>
        /// Update an existing invoice.
        /// </summary>
        public async Task<Invoice> UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    "invoices")
                {
                    Content = invoice.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<Invoice>(cancellationToken);
        }

        /// <summary>
        /// Update an existing line default.
        /// </summary>
        public async Task<InvoiceLineDefault> UpdateLineDefaultAsync(InvoiceLineDefault lineDefault, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await this.nsTcpWtfClient.SendRequestAsync(
                () => new HttpRequestMessage(
                    HttpMethod.Put,
                    "invoices/linedefaults")
                {
                    Content = lineDefault.ToJsonContent()
                },
                cancellationToken);
            return await response.Content.ReadJsonAsAsync<InvoiceLineDefault>(cancellationToken);
        }
    }
}
