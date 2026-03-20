using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Controllers
{
    public sealed partial class LedgerController
    {
        [HttpPost("invoices/{id}/payments")]
        public async Task<IActionResult> CreateInvoicePaymentAsync(long id, [FromBody] InvoicePayment payment, CancellationToken cancellationToken)
        {
            string accountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Invoice invoice = await nsContext.Invoices
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Lines)
                .Include(x => x.Payments)
                .SingleOrDefaultAsync(x => x.AccountName == accountName && x.Id == id, cancellationToken);
            if (invoice == default)
            {
                return this.NotFound();
            }

            payment.AccountName = accountName;
            payment.Invoice = invoice;
            invoice.Payments.Add(payment);

            // Auto-set PaidDate when payments first cover the total.
            if (!invoice.PaidDate.HasValue && invoice.TotalPaid >= invoice.Total)
            {
                invoice.PaidDate = payment.PaymentDate;
            }

            invoice.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            if ((invoice.ProcessedAt.HasValue || invoice.ReadyForProcessing || invoice.ReprocessRegularly) && !invoice.PaidDate.HasValue)
            {
                invoice.ReadyForProcessing = true;
                invoice.ProcessedAt = default;
                await this.TriggerInvoicePdfAsync(invoice, cancellationToken);
                _ = await nsContext.SaveChangesAsync(cancellationToken);
            }

            return this.Ok(invoice);
        }

        [HttpPut("invoices/{id}/payments/{paymentId}")]
        public async Task<IActionResult> UpdateInvoicePaymentAsync(long id, long paymentId, [FromBody] InvoicePayment payment, CancellationToken cancellationToken)
        {
            string accountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Invoice invoice = await nsContext.Invoices
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Lines)
                .Include(x => x.Payments)
                .SingleOrDefaultAsync(x => x.AccountName == accountName && x.Id == id, cancellationToken);
            if (invoice == default)
            {
                return this.NotFound();
            }

            InvoicePayment paymentRecord = invoice.Payments.SingleOrDefault(x => x.Id == paymentId);
            if (paymentRecord == default)
            {
                return this.NotFound($"Payment id [{paymentId}] not found on invoice [{id}].");
            }

            paymentRecord.PaymentDate = payment.PaymentDate;
            paymentRecord.Details = payment.Details;
            paymentRecord.ModifiedDate = DateTimeOffset.UtcNow;

            // Re-derive PaidDate in case the edited payment's date changed when settlement occurred.
            if (invoice.TotalPaid >= invoice.Total)
            {
                decimal cumulative = 0;
                invoice.PaidDate = invoice.Payments
                    .OrderBy(x => x.PaymentDate)
                    .FirstOrDefault(x => (cumulative += x.Amount) >= invoice.Total)
                    ?.PaymentDate;
            }

            invoice.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            if ((invoice.ProcessedAt.HasValue || invoice.ReadyForProcessing || invoice.ReprocessRegularly) && !invoice.PaidDate.HasValue)
            {
                invoice.ReadyForProcessing = true;
                invoice.ProcessedAt = default;
                await this.TriggerInvoicePdfAsync(invoice, cancellationToken);
                _ = await nsContext.SaveChangesAsync(cancellationToken);
            }

            return this.Ok(invoice);
        }

        [HttpDelete("invoices/{id}/payments/{paymentId}")]
        public async Task<IActionResult> DeleteInvoicePaymentAsync(long id, long paymentId, CancellationToken cancellationToken)
        {
            string accountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Invoice invoice = await nsContext.Invoices
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Lines)
                .Include(x => x.Payments)
                .SingleOrDefaultAsync(x => x.AccountName == accountName && x.Id == id, cancellationToken);
            if (invoice == default)
            {
                return this.NotFound();
            }

            InvoicePayment payment = invoice.Payments.SingleOrDefault(x => x.Id == paymentId);
            if (payment == default)
            {
                return this.NotFound($"Payment id [{paymentId}] not found on invoice [{id}].");
            }

            _ = invoice.Payments.Remove(payment);
            _ = nsContext.InvoicePayments.Remove(payment);

            // Clear PaidDate if payments no longer cover the total.
            if (invoice.PaidDate.HasValue && invoice.TotalPaid < invoice.Total)
            {
                invoice.PaidDate = null;
            }

            invoice.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            if ((invoice.ProcessedAt.HasValue || invoice.ReadyForProcessing || invoice.ReprocessRegularly) && !invoice.PaidDate.HasValue)
            {
                invoice.ReadyForProcessing = true;
                invoice.ProcessedAt = default;
                await this.TriggerInvoicePdfAsync(invoice, cancellationToken);
                _ = await nsContext.SaveChangesAsync(cancellationToken);
            }

            return this.Ok(invoice);
        }
    }
}
