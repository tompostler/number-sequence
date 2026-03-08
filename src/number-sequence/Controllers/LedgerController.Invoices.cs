using DurableTask.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Controllers
{
    public sealed partial class LedgerController
    {
        [HttpPost("invoices")]
        public async Task<IActionResult> CreateAsync([FromBody] Invoice invoice, CancellationToken cancellationToken)
        {
            invoice.AccountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Business business = await nsContext.InvoiceBusinesses
                .SingleOrDefaultAsync(x => x.AccountName == invoice.AccountName && x.Id == invoice.Business.Id, cancellationToken);
            if (business == default)
            {
                return this.NotFound($"Business id [{invoice.Business.Id}] not found.");
            }
            invoice.Business = business;

            Customer customer = await nsContext.InvoiceCustomers
                .SingleOrDefaultAsync(x => x.AccountName == invoice.AccountName && x.Id == invoice.Customer.Id, cancellationToken);
            if (customer == default)
            {
                return this.NotFound($"Customer id [{invoice.Customer.Id}] not found.");
            }
            invoice.Customer = customer;

            _ = nsContext.Invoices.Add(invoice);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(invoice);
        }

        [HttpGet("invoices")]
        public async Task<IActionResult> GetsAsync(CancellationToken cancellationToken, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            List<Invoice> invoices = await nsContext.Invoices
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Lines)
                .Where(x => x.AccountName == this.User.Identity.Name)
                .OrderByDescending(x => x.PaidDate ?? DateOnly.MaxValue)
                .ThenByDescending(x => x.ModifiedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
            return this.Ok(invoices);
        }

        [HttpGet("invoices/{id}")]
        public async Task<IActionResult> GetAsync(long id, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Invoice invoice = await nsContext.Invoices
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Lines)
                .SingleOrDefaultAsync(x => x.AccountName == this.User.Identity.Name && x.Id == id, cancellationToken);
            if (invoice == default)
            {
                return this.NotFound();
            }
            else
            {
                return this.Ok(invoice);
            }
        }

        [HttpPut("invoices")]
        public async Task<IActionResult> UpdateAsync([FromBody] Invoice invoice, CancellationToken cancellationToken)
        {
            invoice.AccountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Invoice invoiceRecord = await nsContext.Invoices
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Lines)
                .SingleOrDefaultAsync(x => x.AccountName == invoice.AccountName && x.Id == invoice.Id, cancellationToken);
            if (invoiceRecord == default)
            {
                return this.NotFound("You must create an invoice before trying to update it.");
            }

            Business business = await nsContext.InvoiceBusinesses
                .SingleOrDefaultAsync(x => x.AccountName == invoice.AccountName && x.Id == invoice.Business.Id, cancellationToken);
            if (business == default)
            {
                return this.NotFound($"Business id [{invoice.Business.Id}] not found.");
            }

            Customer customer = await nsContext.InvoiceCustomers
                .SingleOrDefaultAsync(x => x.AccountName == invoice.AccountName && x.Id == invoice.Customer.Id, cancellationToken);
            if (customer == default)
            {
                return this.NotFound($"Customer id [{invoice.Customer.Id}] not found.");
            }

            List<InvoiceLine> invoiceLines = new();
            foreach (InvoiceLine invoiceLine in invoice.Lines)
            {
                InvoiceLine invoiceLineRecord = invoiceRecord.Lines.SingleOrDefault(x => x.Id == invoiceLine.Id);
                if (invoiceLine.Id == default)
                {
                    invoiceLines.Add(invoiceLine);
                }
                else if (invoiceLineRecord == default)
                {
                    return this.NotFound($"Line id [{invoiceLine.Id}] not found.");
                }
                else
                {
                    if (invoiceLineRecord.Title != invoiceLine.Title
                        || invoiceLineRecord.Description != invoiceLine.Description
                        || invoiceLineRecord.Quantity != invoiceLine.Quantity
                        || invoiceLineRecord.Unit != invoiceLine.Unit
                        || invoiceLineRecord.Price != invoiceLine.Price)
                    {
                        invoiceLineRecord.ModifiedDate = DateTimeOffset.UtcNow;
                    }

                    invoiceLineRecord.Title = invoiceLine.Title;
                    invoiceLineRecord.Description = invoiceLine.Description;
                    invoiceLineRecord.Quantity = invoiceLine.Quantity;
                    invoiceLineRecord.Unit = invoiceLine.Unit;
                    invoiceLineRecord.Price = invoiceLine.Price;

                    invoiceLines.Add(invoiceLineRecord);
                }
            }

            invoiceRecord.Title = invoice.Title;
            invoiceRecord.Description = invoice.Description;
            invoiceRecord.DueDate = invoice.DueDate;
            invoiceRecord.Business = business;
            invoiceRecord.Customer = customer;
            invoiceRecord.Lines = invoiceLines;
            invoiceRecord.PaidDate = invoice.PaidDate;
            invoiceRecord.PaidDetails = invoice.PaidDetails;
            invoiceRecord.ReadyForProcessing = invoice.ReadyForProcessing;
            invoiceRecord.ProcessedAt = invoice.ProcessedAt;
            invoiceRecord.ReprocessRegularly = invoice.ReprocessRegularly;

            if ((invoiceRecord.ReadyForProcessing || invoiceRecord.ReprocessRegularly) && invoiceRecord.ProcessedAt == default)
            {
                invoiceRecord.ProccessAttempt += 1;

                TaskHubClient taskHubClient = await this.sentinals.DurableOrchestrationClient.WaitForCompletionAsync(cancellationToken);
                OrchestrationInstance instance = await taskHubClient.CreateOrchestrationInstanceAsync(
                    typeof(DurableTaskImpl.Orchestrators.LedgerInvoiceGenerationOrchestrator),
                    instanceId: $"{invoiceRecord.FriendlyId}_invoice",
                    invoiceRecord.Id);
                this.logger.LogInformation($"Created orchestration {instance.InstanceId} to generate the pdf.");
            }

            invoiceRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(invoiceRecord);
        }
    }
}
