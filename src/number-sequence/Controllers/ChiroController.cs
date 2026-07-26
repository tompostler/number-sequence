using DurableTask.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;
using number_sequence.Models;
using number_sequence.Utilities;
using System.Text.Json;
using TcpWtf.NumberSequence.Contracts;
using Unlimitedinf.Utilities.Extensions;

namespace number_sequence.Controllers
{
    [ApiController, Route("[controller]"), RequiresToken(AccountRoles.Chiro)]
    public sealed class ChiroController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly Sentinals sentinals;
        private readonly ILogger<ChiroController> logger;

        public ChiroController(
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<ChiroController> logger)
        {
            this.serviceProvider = serviceProvider;
            this.sentinals = sentinals;
            this.logger = logger;
        }

        [HttpPost("canine")]
        public async Task<IActionResult> PostCanineAsync([FromBody] ChiroInput input, CancellationToken cancellationToken)
        {
            string validationMessage = ValidateLengths(input, thoracic: 13, ribs: 13, lumbar: 7, lumbarIntertransverse: 0);
            return validationMessage != default
                ? this.BadRequest(validationMessage)
                : await this.SubmitAsync(
                    NsStorage.C.PT.ChiroCanine,
                    typeof(DurableTaskImpl.Orchestrators.ChiroCanineGenerationOrchestrator),
                    input,
                    cancellationToken);
        }

        [HttpPost("equine")]
        public async Task<IActionResult> PostEquineAsync([FromBody] ChiroInput input, CancellationToken cancellationToken)
        {
            string validationMessage = ValidateLengths(input, thoracic: 18, ribs: 18, lumbar: 6, lumbarIntertransverse: 3);
            return validationMessage != default
                ? this.BadRequest(validationMessage)
                : await this.SubmitAsync(
                    NsStorage.C.PT.ChiroEquine,
                    typeof(DurableTaskImpl.Orchestrators.ChiroEquineGenerationOrchestrator),
                    input,
                    cancellationToken);
        }

        /// <summary>
        /// The array lengths are species-specific and the pdf generation activities index into them without
        /// null checks, so reject anything that doesn't line up before it gets recorded.
        /// A <paramref name="lumbarIntertransverse"/> of 0 means the species does not have that data.
        /// </summary>
        private static string ValidateLengths(ChiroInput input, int thoracic, int ribs, int lumbar, int lumbarIntertransverse)
        {
            const int cervical = 7;

            static string check(string name, string[] actual, int expected)
                => (actual?.Length ?? -1) == expected
                    ? default
                    : $"{name} must have exactly {expected} entries but had {(actual == default ? "none" : actual.Length.ToString())}.";

            string message = check(nameof(input.Cervical), input.Cervical, cervical)
                ?? check(nameof(input.Thoracic), input.Thoracic, thoracic)
                ?? check(nameof(input.Ribs), input.Ribs, ribs)
                ?? check(nameof(input.Lumbar), input.Lumbar, lumbar);

            if (message != default)
            {
                return message;
            }

            return lumbarIntertransverse == 0
                ? input.LumbarIntertransverse == default
                    ? default
                    : $"{nameof(input.LumbarIntertransverse)} does not apply to this species and must be omitted."
                : check(nameof(input.LumbarIntertransverse), input.LumbarIntertransverse, lumbarIntertransverse);
        }

        private async Task<IActionResult> SubmitAsync(
            string templateId,
            Type orchestratorType,
            ChiroInput input,
            CancellationToken cancellationToken)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            PdfTemplate template = await nsContext.PdfTemplates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);
            if (template == default)
            {
                this.logger.LogError($"No pdf template defined for {templateId}.");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"No pdf template is configured for {templateId}.");
            }

            // These are never taken from the caller.
            input.RowCreatedAt = DateTimeOffset.UtcNow;
            input.EmailSubmitter = this.User.Identity.Name;
            input.ToEmail = template.EmailTo;

            // Matches the shape of the id the google sheet ingestion computes so that MakeHumanFriendly behaves the same.
            string rowId = $"ui|{templateId}|{input.EmailSubmitter}|{input.RowCreatedAt:O}|{Guid.NewGuid()}".ComputeSHA256();

            // Source must not be a spreadsheet id; the google sheet background services count records by
            // Source == SpreadsheetId to determine how many spreadsheet rows to skip.
            string source = $"ui/{input.EmailSubmitter}";
            if (source.Length > 128)
            {
                source = source[..128];
            }

            ChiroRecord record = new()
            {
                Source = source,
                RowId = rowId,
                DataEnteredAt = input.RowCreatedAt,
                InputJson = JsonSerializer.Serialize(input),
            };
            _ = nsContext.ChiroRecords.Add(record);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            TaskHubClient taskHubClient = await this.sentinals.DurableOrchestrationClient.WaitForCompletionAsync(cancellationToken);
            OrchestrationInstance instance = await taskHubClient.CreateOrchestrationInstanceAsync(
                orchestratorType,
                instanceId: $"{rowId.MakeHumanFriendly()}_{template.Id}",
                record.RowId);
            this.logger.LogInformation($"Created orchestration {instance.InstanceId} to generate the pdf.");

            return this.Ok(new ChiroInputCreated
            {
                RowId = record.RowId,
                OrchestrationInstanceId = instance.InstanceId,
            });
        }
    }
}
