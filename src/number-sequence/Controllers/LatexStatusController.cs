using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Controllers
{
    [ApiController, Route("[controller]"), RequiresToken(AccountRoles.LatexStatus)]
    public sealed class LatexStatusController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        public LatexStatusController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            DateTimeOffset monthAgo = DateTimeOffset.UtcNow.AddDays(-30);

            List<Models.LatexTemplateSpreadsheetRow> latexTemplateSpreadsheetRows = await nsContext.LatexTemplateSpreadsheetRows
                                                                                                    .Where(r => r.CreatedDate > monthAgo)
                                                                                                    .OrderByDescending(r => r.CreatedDate)
                                                                                                    .Take(10)
                                                                                                    .ToListAsync();
            List<Models.LatexDocument> latexDocuments = await nsContext.LatexDocuments
                                                                        .Where(r => r.CreatedDate > monthAgo)
                                                                        .OrderByDescending(r => r.CreatedDate)
                                                                        .Take(10)
                                                                        .ToListAsync();
            List<Models.EmailDocument> emailDocuments = await nsContext.EmailDocuments
                                                                        .Where(r => r.CreatedDate > monthAgo)
                                                                        .OrderByDescending(r => r.CreatedDate)
                                                                        .Take(10)
                                                                        .ToListAsync();
            LatexStatus latexStatus = new()
            {
                LatexTemplateSpreadsheetRows = latexTemplateSpreadsheetRows.Select(
                    r => new LatexStatus.LatexTemplateSpreadsheetRow
                    {
                        RowId = r.RowId,
                        LatexDocumentId = r.LatexDocumentId,
                        CreatedDate = r.CreatedDate.ToString("u"),
                    })
                    .ToList(),
                LatexDocuments = latexDocuments.Select(
                    d => new LatexStatus.LatexDocument
                    {
                        Id = d.Id,
                        CreatedDate = d.CreatedDate.ToString("u"),
                        ProcessedAt = d.ProcessedAt?.ToString("u"),
                        Delay = (d.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(d.CreatedDate).ToString(),
                        Successful = d.Successful?.ToString(),
                    })
                    .ToList(),
                EmailDocuments = emailDocuments.Select(
                    e => new LatexStatus.EmailDocument
                    {
                        Id = e.Id,
                        Subject = e.Subject,
                        AttachmentName = e.AttachmentName,
                        CreatedDate = e.CreatedDate.ToString("u"),
                        ProcessedAt = e.ProcessedAt?.ToString("u"),
                        Delay = (e.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(e.CreatedDate).ToString(),
                    })
                    .ToList()
            };

            return this.Ok(latexStatus);
        }
    }
}
