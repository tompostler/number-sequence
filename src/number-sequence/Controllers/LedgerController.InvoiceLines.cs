using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Controllers
{
    public sealed partial class LedgerController
    {
        [HttpPost("invoices/{id}/lines")]
        public async Task<IActionResult> CreateInvoiceLineAsync(long id, [FromBody] InvoiceLine line, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Invoice invoiceRecord = await nsContext.Invoices
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Lines)
                .Include(x => x.Payments)
                .SingleOrDefaultAsync(x => x.AccountName == this.User.Identity.Name && x.Id == id, cancellationToken);
            if (invoiceRecord == default)
            {
                return this.NotFound($"Invoice id [{id}] not found.");
            }

            line.Id = default;
            invoiceRecord.Lines.Add(line);
            invoiceRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(invoiceRecord);
        }

        [HttpPut("invoices/{id}/lines/{lineId}")]
        public async Task<IActionResult> UpdateInvoiceLineAsync(long id, long lineId, [FromBody] InvoiceLine line, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Invoice invoiceRecord = await nsContext.Invoices
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Lines)
                .Include(x => x.Payments)
                .SingleOrDefaultAsync(x => x.AccountName == this.User.Identity.Name && x.Id == id, cancellationToken);
            if (invoiceRecord == default)
            {
                return this.NotFound($"Invoice id [{id}] not found.");
            }

            InvoiceLine lineRecord = invoiceRecord.Lines.SingleOrDefault(x => x.Id == lineId);
            if (lineRecord == default)
            {
                return this.NotFound($"Line id [{lineId}] not found on invoice [{id}].");
            }

            lineRecord.Title = line.Title;
            lineRecord.Description = line.Description;
            lineRecord.Quantity = line.Quantity;
            lineRecord.Unit = line.Unit;
            lineRecord.Price = line.Price;
            lineRecord.ModifiedDate = DateTimeOffset.UtcNow;

            invoiceRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(invoiceRecord);
        }

        [HttpDelete("invoices/{id}/lines/{lineId}")]
        public async Task<IActionResult> DeleteInvoiceLineAsync(long id, long lineId, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Invoice invoiceRecord = await nsContext.Invoices
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Lines)
                .Include(x => x.Payments)
                .SingleOrDefaultAsync(x => x.AccountName == this.User.Identity.Name && x.Id == id, cancellationToken);
            if (invoiceRecord == default)
            {
                return this.NotFound($"Invoice id [{id}] not found.");
            }

            InvoiceLine lineRecord = invoiceRecord.Lines.SingleOrDefault(x => x.Id == lineId);
            if (lineRecord == default)
            {
                return this.NotFound($"Line id [{lineId}] not found on invoice [{id}].");
            }

            _ = invoiceRecord.Lines.Remove(lineRecord);
            invoiceRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(invoiceRecord);
        }
    }
}
