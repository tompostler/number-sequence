using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Controllers
{
    [ApiController, Route("[controller]"), RequiresToken]
    public sealed class PdfStatusController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        public PdfStatusController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(
            [FromQuery] double hoursOffset = 0,
            [FromQuery] int takeAmount = 20,
            [FromQuery] int daysLookback = 30)
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            DateTimeOffset daysAgo = DateTimeOffset.UtcNow.AddDays(-daysLookback);

            List<Models.ChiroRecord> chiroRecords = [];
            List<Models.ChiroEmailBatch> chiroBatches = [];
            List<Models.ChiroEmailBatch> pendingChiroBatches = [];
            List <Models.EmailDocument> emailDocuments = [];

            bool hasChiroAccess = this.User.IsInRole(AccountRoles.Chiro) || this.User.IsInRole(AccountRoles.PdfStatus);
            bool hasEmailAccess = this.User.IsInRole(AccountRoles.PdfStatus);

            if (hasChiroAccess)
            {
                chiroRecords = await nsContext.ChiroRecords
                                                .Where(r => r.RecordedAt > daysAgo)
                                                .OrderByDescending(r => r.RecordedAt)
                                                .Take(takeAmount)
                                                .ToListAsync();
                chiroBatches = await nsContext.ChiroEmailBatches
                                                .Where(r => r.CreatedDate > daysAgo)
                                                .OrderByDescending(r => r.CreatedDate)
                                                .Take(takeAmount)
                                                .ToListAsync();

                // Not bound by daysLookback/takeAmount - a stuck straggler outside that window should still show up.
                pendingChiroBatches = await nsContext.ChiroEmailBatches
                                                .Where(r => r.ProcessedAt == null)
                                                .ToListAsync();
            }
            if (hasEmailAccess)
            {
                emailDocuments = await nsContext.EmailDocuments
                                                .Where(r => r.CreatedDate > daysAgo)
                                                .OrderByDescending(r => r.CreatedDate)
                                                .Take(takeAmount)
                                                .ToListAsync();
            }

            static string determineTimeSpanFormat(IEnumerable<TimeSpan> spans)
            {
                TimeSpan largest = spans.Any() ? spans.Max() : TimeSpan.Zero;
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
                    return @"mm\:ss\.f";
                }
                else if (largest.TotalMinutes >= 1)
                {
                    return @"m\:ss\.ff";
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

            static TimeSpan chiroRecordDelay(Models.ChiroRecord record)
                => (record.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(record.DataEnteredAt);

            static string formatFileLength(long bytes)
                => bytes >= 1_000_000
                    ? $"{bytes / 1_000_000.0:N2} MB"
                    : $"{bytes / 1_000.0:N2} KB";

            const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string chiroRecordsTimeSpanFormat = determineTimeSpanFormat(chiroRecords.Select(chiroRecordDelay));
            string emailDocumentTimeSpanFormat = determineTimeSpanFormat(emailDocuments.Select(x => (x.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(x.CreatedDate)));
            string chiroBatchTimeSpanFormat = determineTimeSpanFormat(chiroBatches.Select(x => (x.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(x.CreatedDate)));

            PdfStatus pdfStatus = new()
            {
                ChiroRecords = chiroRecords.Select(
                    x => new PdfStatus.ChiroRecord
                    {
                        Id = x.RowId,
                        DataEnteredAt = x.DataEnteredAt.AddHours(hoursOffset).ToString(dateTimeFormat),
                        RecordedAt = x.RecordedAt.AddHours(hoursOffset).ToString(dateTimeFormat),
                        ProcessedAt = x.ProcessedAt?.AddHours(hoursOffset).ToString(dateTimeFormat),
                        Delay = chiroRecordDelay(x).ToString(chiroRecordsTimeSpanFormat),
                    })
                    .ToList(),
                EmailDocuments = emailDocuments.Select(
                    x => new PdfStatus.EmailDocument
                    {
                        Id = x.Id,
                        Subject = x.Subject,
                        AttachmentName = x.AttachmentName,
                        FileLength = formatFileLength(x.FileLength),
                        CreatedDate = x.CreatedDate.AddHours(hoursOffset).ToString(dateTimeFormat),
                        ProcessedAt = x.ProcessedAt?.AddHours(hoursOffset).ToString(dateTimeFormat),
                        Delay = (x.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(x.CreatedDate).ToString(emailDocumentTimeSpanFormat),
                    })
                    .ToList(),
                ChiroBatches = chiroBatches.Select(
                    x => new PdfStatus.ChiroBatch
                    {
                        Id = x.Id,
                        Recipient = x.ClinicAbbreviation ?? x.CcEmail,
                        AttachmentName = x.AttachmentName,
                        FileLength = formatFileLength(x.FileLength),
                        CreatedDate = x.CreatedDate.AddHours(hoursOffset).ToString(dateTimeFormat),
                        ProcessedAt = x.ProcessedAt?.AddHours(hoursOffset).ToString(dateTimeFormat),
                        Delay = (x.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(x.CreatedDate).ToString(chiroBatchTimeSpanFormat),
                    })
                    .ToList(),
                ChiroBatchPendingCounts = pendingChiroBatches
                    .GroupBy(x => x.ClinicAbbreviation ?? x.CcEmail)
                    .Select(g => new PdfStatus.ChiroBatchPendingCount
                    {
                        Recipient = g.Key,
                        Count = g.Count(),
                        TotalFileLength = formatFileLength(g.Sum(x => x.FileLength)),
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList(),
            };

            return this.Ok(pdfStatus);
        }
    }
}
