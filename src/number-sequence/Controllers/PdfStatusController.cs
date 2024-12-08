using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Controllers
{
    [ApiController, Route("[controller]"), RequiresToken(AccountRoles.PdfStatus)]
    public sealed class PdfStatusController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        public PdfStatusController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            DateTimeOffset monthAgo = DateTimeOffset.UtcNow.AddDays(-30);

            List<Models.PdfTemplateSpreadsheetRow> pdfTemplateSpreadsheetRows = await nsContext.PdfTemplateSpreadsheetRows
                                                                                                .Where(r => r.CreatedDate > monthAgo)
                                                                                                .OrderByDescending(r => r.CreatedDate)
                                                                                                .Take(10)
                                                                                                .ToListAsync();
            List<Models.PdfDocument> pdfDocuments = await nsContext.PdfDocuments
                                                                    .Where(r => r.CreatedDate > monthAgo)
                                                                    .OrderByDescending(r => r.CreatedDate)
                                                                    .Take(10)
                                                                    .ToListAsync();
            List<Models.EmailDocument> emailDocuments = await nsContext.EmailDocuments
                                                                        .Where(r => r.CreatedDate > monthAgo)
                                                                        .OrderByDescending(r => r.CreatedDate)
                                                                        .Take(10)
                                                                        .ToListAsync();
            PdfStatus pdfStatus = new()
            {
                TemplateSpreadsheetRows = pdfTemplateSpreadsheetRows.Select(
                    r => new PdfStatus.TemplateSpreadsheetRow
                    {
                        RowId = r.RowId,
                        DocumentId = r.DocumentId,
                        CreatedDate = r.CreatedDate.ToString("u"),
                    })
                    .ToList(),
                Documents = pdfDocuments.Select(
                    d => new PdfStatus.Document
                    {
                        Id = d.Id,
                        CreatedDate = d.CreatedDate.ToString("u"),
                        ProcessedAt = d.ProcessedAt?.ToString("u"),
                        Delay = (d.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(d.CreatedDate).ToString(),
                        Successful = d.Successful?.ToString(),
                    })
                    .ToList(),
                EmailDocuments = emailDocuments.Select(
                    e => new PdfStatus.EmailDocument
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

            return this.Ok(pdfStatus);
        }
    }
}
