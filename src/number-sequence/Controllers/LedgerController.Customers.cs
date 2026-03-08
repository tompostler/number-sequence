using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Controllers
{
    public sealed partial class LedgerController
    {
        [HttpPost("customers")]
        public async Task<IActionResult> CreateCustomerAsync([FromBody] Customer customer, CancellationToken cancellationToken)
        {
            customer.AccountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();
            _ = nsContext.InvoiceCustomers.Add(customer);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(customer);
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomersAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            List<Customer> customers = await nsContext.InvoiceCustomers.Where(x => x.AccountName == this.User.Identity.Name).ToListAsync(cancellationToken);
            return this.Ok(customers);
        }

        [HttpGet("customers/{id}")]
        public async Task<IActionResult> GetCustomerAsync(long id, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Customer customer = await nsContext.InvoiceCustomers.SingleOrDefaultAsync(x => x.AccountName == this.User.Identity.Name && x.Id == id, cancellationToken);
            if (customer == default)
            {
                return this.NotFound();
            }
            else
            {
                return this.Ok(customer);
            }
        }

        [HttpPut("customers")]
        public async Task<IActionResult> UpdateCustomerAsync([FromBody] Customer customer, CancellationToken cancellationToken)
        {
            customer.AccountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();
            Customer customerRecord = await nsContext.InvoiceCustomers.SingleOrDefaultAsync(x => x.AccountName == customer.AccountName && x.Id == customer.Id, cancellationToken);
            if (customerRecord == default)
            {
                return this.NotFound();
            }

            customerRecord.Name = customer.Name;
            customerRecord.AddressLine1 = customer.AddressLine1;
            customerRecord.AddressLine2 = customer.AddressLine2;
            customerRecord.Contact = customer.Contact;

            customerRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(customerRecord);
        }
    }
}
