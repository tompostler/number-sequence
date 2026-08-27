using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using number_sequence.DataAccess;
using number_sequence.Filters;
using System.Text.Json;
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

        [HttpGet("chart")]
        public async Task<IActionResult> GetChartAsync(
            [FromQuery] int daysLookback = 30,
            [FromQuery] double hoursOffset = 0,
            [FromQuery] int width = 2560,
            [FromQuery] int height = 1440,
            CancellationToken cancellationToken = default)
        {
            bool hasChiroAccess = this.User.IsInRole(AccountRoles.Chiro) || this.User.IsInRole(AccountRoles.PdfStatus);
            if (!hasChiroAccess)
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

            List<Models.ChiroRecord> chiroRecords = await nsContext.ChiroRecords
                                            .Where(r => r.RecordedAt > daysAgo)
                                            .ToListAsync(cancellationToken);

            if (chiroRecords.Count == 0)
            {
                return this.NoContent();
            }

            // Bucket by (local day, clinic). Clinic isn't an indexed column - it only exists inside the
            // serialized InputJson - so this deserializes every row in the window. Volumes are small.
            const string noClinicLabel = "(no clinic)";
            static string clinicOf(Models.ChiroRecord record)
            {
                string abbreviation = string.IsNullOrWhiteSpace(record.InputJson)
                    ? null
                    : JsonSerializer.Deserialize<ChiroInput>(record.InputJson)?.ClinicAbbreviation;
                return string.IsNullOrWhiteSpace(abbreviation) ? noClinicLabel : abbreviation;
            }

            Dictionary<(DateTime Day, string Clinic), int> byDayAndClinic = chiroRecords
                .GroupBy(r => (Day: r.RecordedAt.AddHours(hoursOffset).Date, Clinic: clinicOf(r)))
                .ToDictionary(g => g.Key, g => g.Count());

            // Every day in the window gets a position, even with zero forms, so gaps between
            // productive days are visible instead of the bars silently compressing together.
            DateTime todayLocal = DateTimeOffset.UtcNow.AddHours(hoursOffset).Date;
            List<DateTime> days = Enumerable.Range(0, daysLookback)
                .Select(i => todayLocal.AddDays(i - daysLookback + 1))
                .ToList();
            List<string> clinics = byDayAndClinic.Keys.Select(k => k.Clinic).Distinct()
                .OrderBy(c => c == noClinicLabel ? 0 : 1)
                .ThenBy(c => c, StringComparer.OrdinalIgnoreCase)
                .ToList();

            ScottPlot.Palettes.Category10 palette = new();
            List<ScottPlot.Bar> bars = [];
            for (int dayIndex = 0; dayIndex < days.Count; dayIndex++)
            {
                double cumulative = 0;
                for (int clinicIndex = 0; clinicIndex < clinics.Count; clinicIndex++)
                {
                    if (!byDayAndClinic.TryGetValue((days[dayIndex], clinics[clinicIndex]), out int count))
                    {
                        continue;
                    }

                    bars.Add(new ScottPlot.Bar
                    {
                        Position = dayIndex,
                        ValueBase = cumulative,
                        Value = cumulative + count,
                        FillColor = palette.GetColor(clinicIndex),
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

            Dictionary<string, int> totalByClinic = byDayAndClinic
                .GroupBy(kv => kv.Key.Clinic)
                .ToDictionary(g => g.Key, g => g.Sum(kv => kv.Value));
            ScottPlot.LegendItem[] legendItems = [.. clinics.Select((c, i) => new ScottPlot.LegendItem { LabelText = $"{c} ({totalByClinic[c]})", FillColor = palette.GetColor(i) })];
            ScottPlot.Panels.LegendPanel legendPanel = plot.ShowLegend(ScottPlot.Edge.Bottom);
            legendPanel.Legend.ManualItems.AddRange(legendItems);
            legendPanel.Legend.Orientation = ScottPlot.Orientation.Horizontal;

            plot.Title($"Chiro forms per clinic, last {daysLookback} days ({totalByClinic.Values.Sum()})");
            _ = plot.Add.Annotation($"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC", ScottPlot.Alignment.UpperLeft);

            byte[] bytes = plot.GetImage(width, height).GetImageBytes();
            return this.File(bytes, "image/png");
        }
    }
}
