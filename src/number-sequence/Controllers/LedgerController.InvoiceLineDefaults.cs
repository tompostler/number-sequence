using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Controllers
{
    public sealed partial class LedgerController
    {
        [HttpPost("invoicelinedefaults")]
        public async Task<IActionResult> CreateLineDefaultAsync([FromBody] InvoiceLineDefault lineDefault, CancellationToken cancellationToken)
        {
            lineDefault.AccountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();
            _ = nsContext.InvoiceLineDefaults.Add(lineDefault);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(lineDefault);
        }

        [HttpGet("invoicelinedefaults")]
        public async Task<IActionResult> GetLineDefaultsAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            List<InvoiceLineDefault> lineDefaults = await nsContext.InvoiceLineDefaults.Where(x => x.AccountName == this.User.Identity.Name).ToListAsync(cancellationToken);
            return this.Ok(lineDefaults);
        }

        [HttpGet("invoicelinedefaults/{id}")]
        public async Task<IActionResult> GetLineDefaultAsync(long id, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            InvoiceLineDefault lineDefault = await nsContext.InvoiceLineDefaults.SingleOrDefaultAsync(x => x.AccountName == this.User.Identity.Name && x.Id == id, cancellationToken);
            if (lineDefault == default)
            {
                return this.NotFound();
            }
            else
            {
                return this.Ok(lineDefault);
            }
        }

        [HttpPut("invoicelinedefaults")]
        public async Task<IActionResult> UpdateLineDefaultAsync([FromBody] InvoiceLineDefault lineDefault, CancellationToken cancellationToken)
        {
            lineDefault.AccountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();
            InvoiceLineDefault lineDefaultRecord = await nsContext.InvoiceLineDefaults.SingleOrDefaultAsync(x => x.AccountName == lineDefault.AccountName && x.Id == lineDefault.Id, cancellationToken);
            if (lineDefaultRecord == default)
            {
                return this.NotFound();
            }

            lineDefaultRecord.Title = lineDefault.Title;
            lineDefaultRecord.Description = lineDefault.Description;
            lineDefaultRecord.Quantity = lineDefault.Quantity;
            lineDefaultRecord.Unit = lineDefault.Unit;
            lineDefaultRecord.Price = lineDefault.Price;

            lineDefaultRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(lineDefaultRecord);
        }
    }
}
