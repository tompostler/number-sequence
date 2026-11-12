using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;
using number_sequence.Utilities;
using System.Linq.Expressions;
using System.Text.Json;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Controllers
{
    [ApiController, Route("[controller]"), RequiresToken]
    public sealed class PdfStatusController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// The columns of a <see cref="Models.ChiroRecord"/> the status lists show. Projecting to it leaves
        /// <see cref="Models.ChiroRecord.InputJson"/>, the whole filled-out form, in the database.
        /// </summary>
        private sealed record ChiroRecordRow(string RowId, string AccountName, DateTimeOffset DataEnteredAt, DateTimeOffset? DictationParsedAt, DateTimeOffset RecordedAt, DateTimeOffset? ProcessedAt, int ProcessAttempt);

        private static readonly Expression<Func<Models.ChiroRecord, ChiroRecordRow>> toChiroRecordRow =
            r => new ChiroRecordRow(r.RowId, r.AccountName, r.DataEnteredAt, r.DictationParsedAt, r.RecordedAt, r.ProcessedAt, r.ProcessAttempt);

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

            List<ChiroRecordRow> chiroRecords = [];
            List<ChiroRecordRow> pendingChiroRecords = [];
            List<Models.ChiroEmailBatch> chiroBatches = [];
            List<Models.ChiroEmailBatch> pendingChiroBatches = [];
            List<Models.EmailDocument> chiroEmailDocuments = [];
            List<Models.EmailDocument> ledgerEmailDocuments = [];
            List<Models.EmailDocument> unclassifiedEmailDocuments = [];

            // Admin sees every section and every email document, whatever its kind or account.
            bool isAdmin = this.User.IsInRole(AccountRoles.Admin);
            bool hasChiroAccess = isAdmin || this.User.IsInRole(AccountRoles.Chiro);
            bool hasLedgerAccess = this.User.IsInRole(AccountRoles.Ledger);
            string accountName = this.User.Identity.Name;

            if (hasChiroAccess)
            {
                chiroRecords = await nsContext.ChiroRecords
                                                .Where(r => r.RecordedAt > daysAgo)
                                                .OrderByDescending(r => r.RecordedAt)
                                                .Take(takeAmount)
                                                .Select(toChiroRecordRow)
                                                .ToListAsync();
                chiroBatches = await nsContext.ChiroEmailBatches
                                                .AsNoTracking()
                                                .Where(r => r.CreatedDate > daysAgo)
                                                .OrderByDescending(r => r.CreatedDate)
                                                .Take(takeAmount)
                                                .ToListAsync();

                // Not bound by daysLookback/takeAmount - a stuck straggler outside that window should still show up.
                pendingChiroBatches = await nsContext.ChiroEmailBatches
                                                .AsNoTracking()
                                                .Where(r => r.ProcessedAt == null)
                                                .ToListAsync();

                // Same reasoning.
                pendingChiroRecords = await nsContext.ChiroRecords
                                                .Where(r => r.ProcessedAt == null)
                                                .OrderBy(r => r.RecordedAt)
                                                .Select(toChiroRecordRow)
                                                .ToListAsync();
            }
            // One query per kind, so takeAmount applies per section and a busy week of one kind can't push the
            // other off the page.
            Task<List<Models.EmailDocument>> emailDocumentsAsync(Expression<Func<Models.EmailDocument, bool>> filter)
                => nsContext.EmailDocuments
                            .AsNoTracking()
                            .Where(r => r.CreatedDate > daysAgo)
                            .Where(filter)
                            .OrderByDescending(r => r.CreatedDate)
                            .Take(takeAmount)
                            .ToListAsync();

            // Chiro documents are shared by every chiro account, the same as the records above.
            if (hasChiroAccess)
            {
                chiroEmailDocuments = await emailDocumentsAsync(r => r.Kind == Models.EmailDocumentKind.Chiro);
            }
            // Ledger data is partitioned by account, so a ledger account sees only its own.
            if (isAdmin || hasLedgerAccess)
            {
                ledgerEmailDocuments = await emailDocumentsAsync(r => r.Kind == Models.EmailDocumentKind.Ledger && (isAdmin || r.AccountName == accountName));
            }
            // Anything else is a creation site that forgot to set Kind. Admin-only, so a bug surfaces instead of hiding.
            if (isAdmin)
            {
                unclassifiedEmailDocuments = await emailDocumentsAsync(r => r.Kind != Models.EmailDocumentKind.Chiro && r.Kind != Models.EmailDocumentKind.Ledger);
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
                else
                {
                    return @"s\.fff";
                }
            }

            // Measured from RecordedAt, so it is the pipeline's time alone. It was measured from DataEnteredAt until that
            // became the form being opened, which would have added the doctor's time filling it in.
            static TimeSpan chiroRecordDelay(ChiroRecordRow record)
                => (record.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(record.RecordedAt);

            static TimeSpan chiroRecordEntryTime(ChiroRecordRow record)
                => record.RecordedAt.Subtract(record.DataEnteredAt);

            static TimeSpan? chiroRecordReviewTime(ChiroRecordRow record)
                => record.RecordedAt - record.DictationParsedAt;

            static string formatFileLength(long bytes)
                => bytes >= 1_000_000
                    ? $"{bytes / 1_000_000.0:N2} MB"
                    : $"{bytes / 1_000.0:N2} KB";

            const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string chiroRecordsTimeSpanFormat = determineTimeSpanFormat(chiroRecords.Select(chiroRecordDelay));
            // Entry and review share a format so the two columns line up against each other.
            string chiroRecordsEntryTimeSpanFormat = determineTimeSpanFormat(
                chiroRecords.Select(chiroRecordEntryTime).Concat(chiroRecords.Select(chiroRecordReviewTime).OfType<TimeSpan>()));
            string chiroBatchTimeSpanFormat = determineTimeSpanFormat(chiroBatches.Select(x => (x.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(x.CreatedDate)));
            string pendingChiroRecordsTimeSpanFormat = determineTimeSpanFormat(pendingChiroRecords.Select(chiroRecordDelay));

            static TimeSpan emailDocumentDelay(Models.EmailDocument document)
                => (document.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(document.CreatedDate);

            List<PdfStatus.EmailDocument> toEmailDocuments(List<Models.EmailDocument> documents)
            {
                string timeSpanFormat = determineTimeSpanFormat(documents.Select(emailDocumentDelay));
                return documents.Select(
                    x => new PdfStatus.EmailDocument
                    {
                        Id = x.Id,
                        AccountName = x.AccountName,
                        Subject = x.Subject,
                        AttachmentName = x.AttachmentName,
                        FileLength = formatFileLength(x.FileLength),
                        CreatedDate = x.CreatedDate.AddHours(hoursOffset).ToString(dateTimeFormat),
                        ProcessedAt = x.ProcessedAt?.AddHours(hoursOffset).ToString(dateTimeFormat),
                        Delay = emailDocumentDelay(x).ToString(timeSpanFormat),
                    })
                    .ToList();
            }

            PdfStatus pdfStatus = new()
            {
                ChiroRecords = chiroRecords.Select(
                    x => new PdfStatus.ChiroRecord
                    {
                        Id = x.RowId,
                        AccountName = x.AccountName,
                        DataEnteredAt = x.DataEnteredAt.AddHours(hoursOffset).ToString(dateTimeFormat),
                        DictationParsedAt = x.DictationParsedAt?.AddHours(hoursOffset).ToString(dateTimeFormat),
                        RecordedAt = x.RecordedAt.AddHours(hoursOffset).ToString(dateTimeFormat),
                        ProcessedAt = x.ProcessedAt?.AddHours(hoursOffset).ToString(dateTimeFormat),
                        EntryTime = chiroRecordEntryTime(x).ToString(chiroRecordsEntryTimeSpanFormat),
                        ReviewTime = chiroRecordReviewTime(x)?.ToString(chiroRecordsEntryTimeSpanFormat),
                        Delay = chiroRecordDelay(x).ToString(chiroRecordsTimeSpanFormat),
                        ProcessAttempt = x.ProcessAttempt,
                    })
                    .ToList(),
                ChiroEmailDocuments = toEmailDocuments(chiroEmailDocuments),
                LedgerEmailDocuments = toEmailDocuments(ledgerEmailDocuments),
                UnclassifiedEmailDocuments = toEmailDocuments(unclassifiedEmailDocuments),
                ChiroBatches = chiroBatches.Select(
                    x => new PdfStatus.ChiroBatch
                    {
                        Id = x.Id,
                        AccountName = x.AccountName,
                        Recipient = x.ClinicAbbreviation ?? x.CcEmail,
                        AttachmentName = x.AttachmentName,
                        FileLength = formatFileLength(x.FileLength),
                        CreatedDate = x.CreatedDate.AddHours(hoursOffset).ToString(dateTimeFormat),
                        ProcessedAt = x.ProcessedAt?.AddHours(hoursOffset).ToString(dateTimeFormat),
                        Delay = (x.ProcessedAt ?? DateTimeOffset.UtcNow).Subtract(x.CreatedDate).ToString(chiroBatchTimeSpanFormat),
                    })
                    .ToList(),
                ChiroRecordsPending = pendingChiroRecords.Select(
                    x => new PdfStatus.ChiroRecord
                    {
                        Id = x.RowId,
                        AccountName = x.AccountName,
                        DataEnteredAt = x.DataEnteredAt.AddHours(hoursOffset).ToString(dateTimeFormat),
                        RecordedAt = x.RecordedAt.AddHours(hoursOffset).ToString(dateTimeFormat),
                        Delay = chiroRecordDelay(x).ToString(pendingChiroRecordsTimeSpanFormat),
                        ProcessAttempt = x.ProcessAttempt,
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

        [HttpGet("chart")]
        public async Task<IActionResult> GetChartAsync(
            [FromQuery] int daysLookback = 30,
            [FromQuery] double hoursOffset = 0,
            [FromQuery] int width = 2560,
            [FromQuery] int height = 1440,
            [FromQuery] ChiroChartGrouping groupBy = ChiroChartGrouping.Clinic,
            CancellationToken cancellationToken = default)
        {
            if (!this.User.IsInRole(AccountRoles.Admin) && !this.User.IsInRole(AccountRoles.Chiro))
            {
                return this.Forbid();
            }

            if (width < 100 || width > 2560 || height < 100 || height > 1440)
            {
                return this.BadRequest($"Width must be 100–2560 and height must be 100–1440.");
            }

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            DateTimeOffset daysAgo = DateTimeOffset.UtcNow.AddDays(-daysLookback);

            IQueryable<Models.ChiroRecord> inWindow = nsContext.ChiroRecords.Where(r => r.RecordedAt > daysAgo);

            // Each record becomes (recorded at, segment label). A record with no value gets a
            // placeholder label rather than being dropped, so day totals still match total submissions.
            string noneLabel;
            List<(DateTimeOffset RecordedAt, string Group)> chiroRecords;
            if (groupBy == ChiroChartGrouping.Account)
            {
                noneLabel = "(no account)";
                chiroRecords = (await inWindow
                                    .Select(r => new { r.RecordedAt, r.AccountName })
                                    .ToListAsync(cancellationToken))
                    .Select(r => (r.RecordedAt, string.IsNullOrWhiteSpace(r.AccountName) ? noneLabel : r.AccountName))
                    .ToList();
            }
            else
            {
                // Clinic isn't an indexed column - it only exists inside the serialized InputJson - so
                // this deserializes every row in the window. Volumes are small.
                noneLabel = "(no clinic)";
                string clinicOf(string inputJson)
                {
                    string abbreviation = string.IsNullOrWhiteSpace(inputJson)
                        ? null
                        : JsonSerializer.Deserialize<ChiroInput>(inputJson, RelaxedJson.Options)?.ClinicAbbreviation;
                    return string.IsNullOrWhiteSpace(abbreviation) ? noneLabel : abbreviation;
                }

                chiroRecords = (await inWindow
                                    .Select(r => new { r.RecordedAt, r.InputJson })
                                    .ToListAsync(cancellationToken))
                    .Select(r => (r.RecordedAt, clinicOf(r.InputJson)))
                    .ToList();
            }

            if (chiroRecords.Count == 0)
            {
                return this.NoContent();
            }

            Dictionary<(DateTime Day, string Group), int> byDayAndGroup = chiroRecords
                .GroupBy(r => (Day: r.RecordedAt.AddHours(hoursOffset).Date, r.Group))
                .ToDictionary(g => g.Key, g => g.Count());

            // Every day in the window gets a position, even with zero forms, so gaps between
            // productive days are visible instead of the bars silently compressing together.
            DateTime todayLocal = DateTimeOffset.UtcNow.AddHours(hoursOffset).Date;
            List<DateTime> days = Enumerable.Range(0, daysLookback)
                .Select(i => todayLocal.AddDays(i - daysLookback + 1))
                .ToList();
            Dictionary<string, int> totalByGroup = byDayAndGroup
                .GroupBy(kv => kv.Key.Group)
                .ToDictionary(g => g.Key, g => g.Sum(kv => kv.Value));

            // The placeholder always sorts first, so it reads as a deliberate category rather than clutter.
            // Clinics are a short fixed set and read best alphabetically; accounts are ordered by volume,
            // since who submitted the most is the question that chart answers.
            IOrderedEnumerable<string> orderedGroups = totalByGroup.Keys.OrderBy(g => g == noneLabel ? 0 : 1);
            orderedGroups = groupBy == ChiroChartGrouping.Account
                ? orderedGroups.ThenByDescending(g => totalByGroup[g]).ThenBy(g => g, StringComparer.OrdinalIgnoreCase)
                : orderedGroups.ThenBy(g => g, StringComparer.OrdinalIgnoreCase);
            List<string> groups = orderedGroups.ToList();

            ScottPlot.Palettes.Category10 palette = new();
            List<ScottPlot.Bar> bars = [];
            for (int dayIndex = 0; dayIndex < days.Count; dayIndex++)
            {
                double cumulative = 0;
                for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
                {
                    if (!byDayAndGroup.TryGetValue((days[dayIndex], groups[groupIndex]), out int count))
                    {
                        continue;
                    }

                    bars.Add(new ScottPlot.Bar
                    {
                        Position = dayIndex,
                        ValueBase = cumulative,
                        Value = cumulative + count,
                        FillColor = palette.GetColor(groupIndex),
                    });
                    cumulative += count;
                }
            }

            ScottPlot.Plot plot = new();
            _ = plot.Add.Bars(bars);

            // Thin x-axis labels for wide lookback windows so they stop overlapping - bars stay one per
            // day regardless, only the labeling gets sparser. Step is derived from the chart's pixel
            // width so it scales for any daysLookback/width combination instead of a fixed cadence.
            // Anchored on the most recent day so "today" always has a label.
            int maxLabels = Math.Max(1, width / 40);
            int labelStep = Math.Max(1, (int)Math.Ceiling(days.Count / (double)maxLabels));
            List<ScottPlot.Tick> tickList = [];
            for (int i = days.Count - 1; i >= 0; i -= labelStep)
            {
                tickList.Add(new ScottPlot.Tick(i, days[i].ToString("MM-dd")));
            }
            tickList.Reverse();
            plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual([.. tickList]);
            plot.Axes.Bottom.MajorTickStyle.Length = 0;
            // Bars are stacked from zero - without this, the y-axis pads below zero by default and
            // the bars appear to float above the x-axis instead of sitting flush on it.
            plot.Axes.Margins(bottom: 0);

            ScottPlot.LegendItem[] legendItems = [.. groups.Select((g, i) => new ScottPlot.LegendItem { LabelText = $"{g} ({totalByGroup[g]})", FillColor = palette.GetColor(i) })];
            ScottPlot.Panels.LegendPanel legendPanel = plot.ShowLegend(ScottPlot.Edge.Bottom);
            legendPanel.Legend.ManualItems.AddRange(legendItems);
            legendPanel.Legend.Orientation = ScottPlot.Orientation.Horizontal;

            plot.Title($"Chiro forms per {(groupBy == ChiroChartGrouping.Account ? "account" : "clinic")}, last {daysLookback} days ({totalByGroup.Values.Sum()})");
            _ = plot.Add.Annotation($"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC", ScottPlot.Alignment.UpperLeft);

            byte[] bytes = plot.GetImage(width, height).GetImageBytes();
            return this.File(bytes, "image/png");
        }
    }
}
