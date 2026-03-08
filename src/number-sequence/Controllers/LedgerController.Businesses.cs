using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Controllers
{
    public sealed partial class LedgerController
    {
        [HttpPost("businesses")]
        public async Task<IActionResult> CreateBusinessAsync([FromBody] Business business, CancellationToken cancellationToken)
        {
            business.AccountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();
            _ = nsContext.InvoiceBusinesses.Add(business);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(business);
        }

        [HttpGet("businesses")]
        public async Task<IActionResult> GetBusinessesAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            List<Business> businesses = await nsContext.InvoiceBusinesses.Where(x => x.AccountName == this.User.Identity.Name).ToListAsync(cancellationToken);
            return this.Ok(businesses);
        }

        [HttpGet("businesses/{id}")]
        public async Task<IActionResult> GetBusinessAsync(long id, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Business business = await nsContext.InvoiceBusinesses.SingleOrDefaultAsync(x => x.AccountName == this.User.Identity.Name && x.Id == id, cancellationToken);
            if (business == default)
            {
                return this.NotFound();
            }
            else
            {
                return this.Ok(business);
            }
        }

        [HttpPut("businesses")]
        public async Task<IActionResult> UpdateBusinessAsync([FromBody] Business business, CancellationToken cancellationToken)
        {
            business.AccountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();
            Business businessRecord = await nsContext.InvoiceBusinesses.SingleOrDefaultAsync(x => x.AccountName == business.AccountName && x.Id == business.Id, cancellationToken);
            if (businessRecord == default)
            {
                return this.NotFound();
            }

            businessRecord.Name = business.Name;
            businessRecord.AddressLine1 = business.AddressLine1;
            businessRecord.AddressLine2 = business.AddressLine2;
            businessRecord.Contact = business.Contact;

            businessRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(businessRecord);
        }
    }
}
