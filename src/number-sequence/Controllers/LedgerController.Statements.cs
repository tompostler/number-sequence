using DurableTask.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using TcpWtf.NumberSequence.Contracts.Ledger;

namespace number_sequence.Controllers
{
    public sealed partial class LedgerController
    {
        [HttpPost("statements")]
        public async Task<IActionResult> CreateStatementAsync([FromBody] Statement statement, CancellationToken cancellationToken)
        {
            statement.AccountName = this.User.Identity.Name;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Business business = await nsContext.InvoiceBusinesses
                .SingleOrDefaultAsync(x => x.AccountName == statement.AccountName && x.Id == statement.Business.Id, cancellationToken);
            if (business == default)
            {
                return this.NotFound($"Business id [{statement.Business.Id}] not found.");
            }
            statement.Business = business;

            Customer customer = await nsContext.InvoiceCustomers
                .SingleOrDefaultAsync(x => x.AccountName == statement.AccountName && x.Id == statement.Customer.Id, cancellationToken);
            if (customer == default)
            {
                return this.NotFound($"Customer id [{statement.Customer.Id}] not found.");
            }
            statement.Customer = customer;

            List<Invoice> invoices = await nsContext.Invoices
                .Where(x => x.AccountName == statement.AccountName)
                .Where(x => x.Business == statement.Business)
                .Where(x => x.Customer == statement.Customer)
                .Where(x => statement.SearchByDueDate
                    ? x.DueDate >= statement.InvoiceStartDate && x.DueDate <= statement.InvoiceEndDate
                    : x.CreatedDate > statement.InvoiceStartDate.ToDateTime(TimeOnly.MinValue) && x.CreatedDate < statement.InvoiceEndDate.ToDateTime(TimeOnly.MaxValue))
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Lines)
                .ToListAsync(cancellationToken);
            if (invoices.Count == 0)
            {
                return this.BadRequest($"There are no returned invoices with the period [{statement.InvoiceStartDate}, {statement.InvoiceEndDate}].");
            }

            statement.Invoices = invoices;

            _ = nsContext.Statements.Add(statement);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            if (statement.ReadyForProcessing && statement.ProcessedAt == default)
            {
                statement.ProccessAttempt += 1;

                TaskHubClient taskHubClient = await this.sentinals.DurableOrchestrationClient.WaitForCompletionAsync(cancellationToken);
                OrchestrationInstance instance = await taskHubClient.CreateOrchestrationInstanceAsync(
                    typeof(DurableTaskImpl.Orchestrators.LedgerStatementGenerationOrchestrator),
                    instanceId: $"{statement.FriendlyId}_statement",
                    statement.Id);
                this.logger.LogInformation($"Created orchestration {instance.InstanceId} to generate the pdf.");
            }

            statement.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(statement);
        }

        [HttpGet("statements")]
        public async Task<IActionResult> GetStatementsAsync(CancellationToken cancellationToken, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            List<Statement> statements = await nsContext.Statements
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Invoices)
                    .ThenInclude(x => x.Business)
                .Include(x => x.Invoices)
                    .ThenInclude(x => x.Customer)
                .Include(x => x.Invoices)
                    .ThenInclude(x => x.Lines)
                .Where(x => x.AccountName == this.User.Identity.Name)
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
            return this.Ok(statements);
        }

        [HttpGet("statements/{id}")]
        public async Task<IActionResult> GetStatementAsync(long id, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Statement statement = await nsContext.Statements
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Invoices)
                    .ThenInclude(x => x.Lines)
                .SingleOrDefaultAsync(x => x.AccountName == this.User.Identity.Name && x.Id == id, cancellationToken);
            if (statement == default)
            {
                return this.NotFound();
            }
            else
            {
                return this.Ok(statement);
            }
        }

        [HttpPut("statements/{id}/process")]
        public async Task<IActionResult> UpdateStatementAsync(long id, CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            Statement statementRecord = await nsContext.Statements
                .Include(x => x.Business)
                .Include(x => x.Customer)
                .Include(x => x.Invoices)
                    .ThenInclude(x => x.Lines)
                .SingleOrDefaultAsync(x => x.AccountName == this.User.Identity.Name && x.Id == id, cancellationToken);
            if (statementRecord == default)
            {
                return this.NotFound();
            }

            statementRecord.ReadyForProcessing = true;
            statementRecord.ProcessedAt = default;

            statementRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            if (statementRecord.ReadyForProcessing && statementRecord.ProcessedAt == default)
            {
                statementRecord.ProccessAttempt += 1;

                TaskHubClient taskHubClient = await this.sentinals.DurableOrchestrationClient.WaitForCompletionAsync(cancellationToken);
                OrchestrationInstance instance = await taskHubClient.CreateOrchestrationInstanceAsync(
                    typeof(DurableTaskImpl.Orchestrators.LedgerStatementGenerationOrchestrator),
                    instanceId: $"{statementRecord.FriendlyId}_statement",
                    statementRecord.Id);
                this.logger.LogInformation($"Created orchestration {instance.InstanceId} to generate the pdf.");
            }

            statementRecord.ModifiedDate = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            return this.Ok(statementRecord);
        }
    }
}
