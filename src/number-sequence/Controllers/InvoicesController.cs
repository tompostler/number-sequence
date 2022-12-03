using DurableTask.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using number_sequence.DataAccess;
using number_sequence.Filters;
using number_sequence.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Invoicing;

namespace number_sequence.Controllers
{
    [ApiController, Route("[controller]"), RequiresToken(AccountRoles.LatexStatus)]
    public sealed class InvoicesController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly Sentinals sentinals;
        private readonly ILogger<InvoicesController> logger;

        public InvoicesController(
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<InvoicesController> logger)
        {
            this.serviceProvider = serviceProvider;
            this.sentinals = sentinals;
            this.logger = logger;
        }

        #region Businesses

        [HttpPost("businesses")]
        public async Task<IActionResult> CreateBusinessAsync([FromBody] InvoiceBusiness business, CancellationToken cancellationToken)
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

            List<InvoiceBusiness> businesses = await nsContext.InvoiceBusinesses.Where(x => x.AccountName == this.User.Identity.Name).ToListAsync(cancellationToken);
            return this.Ok(businesses);
        }

        [HttpGet("businesses/{id}")]
        public async Task<IActionResult> GetBusinessAsync(long id, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            InvoiceBusiness business = await nsContext.InvoiceBusinesses.SingleOrDefaultAsync(x => x.AccountName == this.User.Identity.Name && x.Id == id, cancellationToken);
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
        public async Task<IActionResult> UpdateBusinessAsync([FromBody] InvoiceBusiness business, CancellationToken cancellationToken)
        {
            business.AccountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();
            InvoiceBusiness businessRecord = await nsContext.InvoiceBusinesses.SingleOrDefaultAsync(x => x.AccountName == business.AccountName && x.Id == business.Id, cancellationToken);
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

        #endregion // Businesses

        #region Customers

        [HttpPost("customers")]
        public async Task<IActionResult> CreateCustomerAsync([FromBody] InvoiceCustomer customer, CancellationToken cancellationToken)
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

            List<InvoiceCustomer> customers = await nsContext.InvoiceCustomers.Where(x => x.AccountName == this.User.Identity.Name).ToListAsync(cancellationToken);
            return this.Ok(customers);
        }

        [HttpGet("customers/{id}")]
        public async Task<IActionResult> GetCustomerAsync(long id, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            InvoiceCustomer customer = await nsContext.InvoiceCustomers.SingleOrDefaultAsync(x => x.AccountName == this.User.Identity.Name && x.Id == id, cancellationToken);
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
        public async Task<IActionResult> UpdateCustomerAsync([FromBody] InvoiceCustomer customer, CancellationToken cancellationToken)
        {
            customer.AccountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();
            InvoiceCustomer customerRecord = await nsContext.InvoiceCustomers.SingleOrDefaultAsync(x => x.AccountName == customer.AccountName && x.Id == customer.Id, cancellationToken);
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

        #endregion // Customers

        #region Invoices

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] Invoice invoice, CancellationToken cancellationToken)
        {
            invoice.AccountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();


            InvoiceBusiness invoiceBusiness = await nsContext.InvoiceBusinesses
                .SingleOrDefaultAsync(x => x.AccountName == invoice.AccountName && x.Id == invoice.Business.Id, cancellationToken);
            if (invoiceBusiness == default)
            {
                return this.NotFound($"Business id [{invoice.Business.Id}] not found.");
            }
            invoice.Business = invoiceBusiness;

            InvoiceCustomer invoiceCustomer = await nsContext.InvoiceCustomers
                .SingleOrDefaultAsync(x => x.AccountName == invoice.AccountName && x.Id == invoice.Business.Id, cancellationToken);
            if (invoiceCustomer == default)
            {
                return this.NotFound($"Customer id [{invoice.Customer.Id}] not found.");
            }
            invoice.Customer = invoiceCustomer;

            _ = nsContext.Invoices.Add(invoice);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(invoice);
        }

        [HttpGet]
        public async Task<IActionResult> GetsAsync(CancellationToken cancellationToken, [FromQuery]int skip = 0, [FromQuery]int take = 10)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            List<Invoice> invoices = await nsContext.Invoices
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Lines)
                .Where(x => x.AccountName == this.User.Identity.Name)
                .OrderByDescending(x => x.ModifiedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
            return this.Ok(invoices);
        }

        [HttpGet("{id}")]
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

        [HttpPut]
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

            InvoiceBusiness invoiceBusiness = await nsContext.InvoiceBusinesses
                .SingleOrDefaultAsync(x => x.AccountName == invoice.AccountName && x.Id == invoice.Business.Id, cancellationToken);
            if (invoiceBusiness == default)
            {
                return this.NotFound($"Business id [{invoice.Business.Id}] not found.");
            }

            InvoiceCustomer invoiceCustomer = await nsContext.InvoiceCustomers
                .SingleOrDefaultAsync(x => x.AccountName == invoice.AccountName && x.Id == invoice.Customer.Id, cancellationToken);
            if (invoiceCustomer == default)
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
            invoiceRecord.Business = invoiceBusiness;
            invoiceRecord.Customer = invoiceCustomer;
            invoiceRecord.Lines = invoiceLines;
            invoiceRecord.PaidDate = invoice.PaidDate;
            invoiceRecord.PaidDetails = invoice.PaidDetails;
            invoiceRecord.ReadyForProcessing = invoice.ReadyForProcessing;
            invoiceRecord.ProcessedAt = invoice.ProcessedAt;

            if (invoiceRecord.ReadyForProcessing && invoiceRecord.ProcessedAt == default)
            {
                invoiceRecord.ProccessAttempt += 1;

                TaskHubClient taskHubClient = await this.sentinals.DurableOrchestrationClient.WaitForCompletionAsync(cancellationToken);
                OrchestrationInstance instance = await taskHubClient.CreateOrchestrationInstanceAsync(
                    typeof(DurableTaskImpl.Orchestrators.InvoicePostlerOrchestrator),
                    instanceId: $"{invoiceRecord.Id:0000}-{invoiceRecord.ProccessAttempt:00}_{NsStorage.C.LTBP.InvoicePostler}",
                    invoiceRecord.Id);
                this.logger.LogInformation($"Created orchestration {instance.InstanceId} to generate the pdf.");
            }

            invoiceRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(invoiceRecord);
        }

        #endregion // Invoices

        #region Line Defaults

        [HttpPost("linedefaults")]
        public async Task<IActionResult> CreateLineDefaultAsync([FromBody] InvoiceLineDefault lineDefault, CancellationToken cancellationToken)
        {
            lineDefault.AccountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();
            _ = nsContext.InvoiceLineDefaults.Add(lineDefault);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(lineDefault);
        }

        [HttpGet("linedefaults")]
        public async Task<IActionResult> GetLineDefaultsAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            List<InvoiceLineDefault> lineDefaults = await nsContext.InvoiceLineDefaults.Where(x => x.AccountName == this.User.Identity.Name).ToListAsync(cancellationToken);
            return this.Ok(lineDefaults);
        }

        [HttpGet("linedefaults/{id}")]
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

        [HttpPut("linedefaults")]
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

        #endregion // Line Defaults
    }
}
