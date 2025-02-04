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

            static string determineTimeSpanFormat(IEnumerable<TimeSpan> spans)
            {
                TimeSpan largest = spans.Max();
                if (largest.TotalDays >= 10)
                {
                    return @"dd\.hh\:mm\:ss";
                }
                else if (largest.TotalDays >= 1)
                {
                    return @"d\.hh\:mm\:ss";
                }
                else if (largest.TotalHours >= 10)
                {
                    return @"hh\:mm\:ss";
                }
                else if (largest.TotalHours >= 1)
                {
                    return @"h\:mm\:ss";
                }
                else if (largest.TotalMinutes >= 10)
                {
                    return @"mm\:ss\.fff";
                }
                else if (largest.TotalMinutes >= 1)
                {
                    return @"m\:ss\.fff";
                }
                else if (largest.TotalSeconds >= 10)
                {
                    return @"ss\.fff";
                }
                else if (largest.TotalSeconds >= 1)
                {
                    return @"s\.fff";
                }
                else
                {
                    return @"dd\.hh\:mm\:ss\.fff";
                }
            }

            string emailDocumentTimeSpanFormat = determineTimeSpanFormat(emailDocuments.Select(x => (x.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(x.CreatedDate)));
            string chiroBatchTimeSpanFormat = determineTimeSpanFormat(chiroBatches.Select(x => (x.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(x.CreatedDate)));

            PdfStatus pdfStatus = new()
            {
                TemplateSpreadsheetRows = pdfTemplateSpreadsheetRows.Select(
                    x => new PdfStatus.TemplateSpreadsheetRow
                    {
                        RowId = x.RowId,
                        DocumentId = x.DocumentId,
                        CreatedDate = x.CreatedDate.ToString("u"),
                    })
                    .ToList(),
                EmailDocuments = emailDocuments.Select(
                    x => new PdfStatus.EmailDocument
                    {
                        Id = x.Id,
                        Subject = x.Subject,
                        AttachmentName = x.AttachmentName,
                        CreatedDate = x.CreatedDate.ToString("u"),
                        ProcessedAt = x.ProcessedAt?.ToString("u"),
                        Delay = (x.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(x.CreatedDate).ToString(emailDocumentTimeSpanFormat),
                    })
                    .ToList(),
                ChiroBatches = chiroBatches.Select(
                    x => new PdfStatus.ChiroBatch
                    {
                        Id = x.Id,
                        Clinic = x.ClinicAbbreviation,
                        AttachmentName = x.AttachmentName,
                        CreatedDate = x.CreatedDate.ToString("u"),
                        ProcessedAt = x.ProcessedAt?.ToString("u"),
                        Delay = (x.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(x.CreatedDate).ToString(chiroBatchTimeSpanFormat),
                    })
                    .ToList(),
            };

            return this.Ok(pdfStatus);
        }
    }
}
