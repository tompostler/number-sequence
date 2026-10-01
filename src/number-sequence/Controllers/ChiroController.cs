using DurableTask.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;
using number_sequence.Models;
using number_sequence.Utilities;
using System.Text.Json;
using TcpWtf.NumberSequence.Contracts;

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

        [HttpPost("{species}")]
        public async Task<IActionResult> PostAsync(ChiroSpecies species, [FromBody] ChiroInput input, CancellationToken cancellationToken)
        {
            if (!ChiroSpeciesDefinition.TryGet(species, out ChiroSpeciesDefinition definition))
            {
                return this.BadRequest($"{species} is not a supported species.");
            }

            string validationMessage = ValidateLengths(input, definition);
            return validationMessage != default
                ? this.BadRequest(validationMessage)
                : await this.SubmitAsync(definition, input, cancellationToken);
        }

        /// <summary>
        /// The array lengths are species-specific and the pdf generation activity indexes into them without
        /// null checks, so reject anything that doesn't line up before it gets recorded.
        /// A <see cref="ChiroSpeciesDefinition.LumbarIntertransverseCount"/> of 0 means the species does not
        /// have that data.
        /// </summary>
        private static string ValidateLengths(ChiroInput input, ChiroSpeciesDefinition definition)
        {
            const int cervical = 7;

            static string check(string name, string[] actual, int expected)
                => (actual?.Length ?? -1) == expected
                    ? default
                    : $"{name} must have exactly {expected} entries but had {(actual == default ? "none" : actual.Length.ToString())}.";

            string message = check(nameof(input.Cervical), input.Cervical, cervical)
                ?? check(nameof(input.Thoracic), input.Thoracic, definition.ThoracicCount)
                ?? check(nameof(input.Ribs), input.Ribs, definition.RibsCount)
                ?? check(nameof(input.Lumbar), input.Lumbar, definition.LumbarCount);

            if (message != default)
            {
                return message;
            }

            return definition.LumbarIntertransverseCount == 0
                ? input.LumbarIntertransverse == default
                    ? default
                    : $"{nameof(input.LumbarIntertransverse)} does not apply to this species and must be omitted."
                : check(nameof(input.LumbarIntertransverse), input.LumbarIntertransverse, definition.LumbarIntertransverseCount);
        }

        private async Task<IActionResult> SubmitAsync(
            ChiroSpeciesDefinition definition,
            ChiroInput input,
            CancellationToken cancellationToken)
        {
            string templateId = definition.TemplateId;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            PdfTemplate template = await nsContext.PdfTemplates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);
            if (template == default)
            {
                this.logger.LogError($"No pdf template defined for {templateId}.");
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"No pdf template is configured for {templateId}.");
            }

            // These are never taken from the caller.
            input.EmailSubmitter = this.User.Identity.Name;
            input.ToEmail = template.EmailTo;
            input.Species = definition.Species;

            string rowId = Guid.NewGuid().ToString("N");
            string source = $"ui/{input.EmailSubmitter}";
            if (source.Length > 128)
            {
                source = source[..128];
            }

            ChiroRecord record = new()
            {
                Source = source,
                RowId = rowId,
                DataEnteredAt = DateTimeOffset.UtcNow,
                InputJson = JsonSerializer.Serialize(input, RelaxedJson.Options),
            };
            _ = nsContext.ChiroRecords.Add(record);
            _ = await nsContext.SaveChangesAsync(cancellationToken);

            TaskHubClient taskHubClient = await this.sentinals.DurableOrchestrationClient.WaitForCompletionAsync(cancellationToken);
            OrchestrationInstance instance = await taskHubClient.CreateOrchestrationInstanceAsync(
                typeof(DurableTaskImpl.Orchestrators.ChiroGenerationOrchestrator),
                instanceId: ChiroOrchestrationNaming.InstanceId(rowId, template.Id, record.ProcessAttempt),
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
