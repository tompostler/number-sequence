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
            List<Models.EmailDocument> emailDocuments = await nsContext.EmailDocuments
                                                                        .Where(r => r.CreatedDate > monthAgo)
                                                                        .OrderByDescending(r => r.CreatedDate)
                                                                        .Take(10)
                                                                        .ToListAsync();
            List<Models.ChiroEmailBatch> chiroBatches = await nsContext.ChiroEmailBatches
                                                                        .Where(r => r.CreatedDate > monthAgo)
                                                                        .OrderByDescending(r => r.CreatedDate)
                                                                        .Take(20)
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
                EmailDocuments = emailDocuments.Select(
                    e => new PdfStatus.EmailDocument
                    {
                        Id = e.Id,
                        Subject = e.Subject,
                        AttachmentName = e.AttachmentName,
                        CreatedDate = e.CreatedDate.ToString("u"),
                        ProcessedAt = e.ProcessedAt?.ToString("u"),
                        Delay = (e.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(e.CreatedDate).ToString(@"dd\.hh\:mm\:ss\.fff"),
                    })
                    .ToList(),
                ChiroBatches = chiroBatches.Select(
                    c => new PdfStatus.ChiroBatch
                    {
                        Id = c.Id,
                        Clinic = c.ClinicAbbreviation,
                        AttachmentName = c.AttachmentName,
                        CreatedDate = c.CreatedDate.ToString("u"),
                        ProcessedAt = c.ProcessedAt?.ToString("u"),
                        Delay = (c.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(c.CreatedDate).ToString(@"dd\.hh\:mm\:ss\.fff"),
                    })
                    .ToList(),
            };

            return this.Ok(pdfStatus);
        }
    }
}
