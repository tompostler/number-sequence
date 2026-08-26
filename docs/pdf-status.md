# PDF status page

`/ui/pdf-status` (backed by `PdfStatusController`) shows the health of the chiro/pdf background
pipeline: chiro records waiting for pdf generation, email documents waiting to send, and chiro
batch rows (one row per clinic/CC destination per generated pdf) waiting to be sent out.

## Chiro batch pending counts

The "Chiro Batches" table is windowed by `daysLookback`/`takeAmount` like the other sections, so a
straggler that has been stuck pending for longer than the lookback window silently drops off the
page. The "Chiro Batch Pending Counts" section exists to catch that: it queries
`ChiroEmailBatches` for `ProcessedAt == null` with no date/take bound, then groups by
`(ClinicAbbreviation, CcEmail)` and counts. That tuple is always populated on exactly one side per
row — `ChiroPdfGenerationActivity` writes a clinic row and a separate row per CC address, never
both on the same row — so grouping by the pair is equivalent to grouping by destination.

This section is what should be checked first when `ChiroBatchSendBackgroundService` looks like
it isn't draining a clinic's queue: it shows the true backlog per destination independent of the
list-view paging.

## Chiro forms per clinic chart

The "Chiro Forms Per Clinic" section renders a PNG stacked bar chart, one bar per day over the
page's `daysLookback` window, each bar segmented by clinic. It follows the same pattern as the
counts detail chart (`Pages/UI/Counts/Chart.cshtml`): a plain `<img>` tag pointing at a dedicated
Razor Page (`/ui/pdf-status/chart`) that calls the Client, which calls `GET pdfstatus/chart` on
`PdfStatusController`, which draws the plot with ScottPlot and returns `image/png`. The browser
fetches it as a second request after the table-driven part of the page has already rendered - there
is no JS charting library and no JSON chart-data endpoint anywhere in this codebase.

The chart counts from `ChiroRecords` (`RecordedAt` bucketed to the local day via `hoursOffset`,
one row per form submission), not from `ChiroEmailBatches`. `ChiroEmailBatches` has a real indexed
`ClinicAbbreviation` column and is cheaper to query, but it's a downstream artifact of pdf
generation, not the submission itself: it gets a duplicate row per CC recipient, and a record whose
`ClinicAbbreviation` was never set is simply absent. Counting `ChiroRecords` instead answers "forms
filled out per clinic," matching what the section title says, at the cost of deserializing every
row's `InputJson` per request to read `ClinicAbbreviation` (it isn't an indexed column - see
`ChiroRecord`). Volumes are small enough that this is fine; don't switch to `ChiroEmailBatches` to
save the deserialization without also renaming the section, since the two sources measure different
things. A record with no clinic set buckets into a `(no clinic)` clinic rather than being dropped,
so day totals still match total submissions - it's sorted first in the stack/legend rather than
falling wherever it lands alphabetically, so it reads as a deliberate category rather than clutter.

Every day in the `daysLookback` window gets a bar position, including days with zero forms - the
gap is the point, since it's what conveys the spacing between productive days. Don't collapse the
x-axis down to only the days that have data.

The legend is a `ScottPlot.Panels.LegendPanel` (from `plot.ShowLegend(Edge.Bottom)`), which renders
outside the plot's data area below the x-axis, not `plot.ShowLegend(items)` alone - the latter draws
inside the data area and can overlap the bars. Manual items (one per clinic, carrying the color) go
on `legendPanel.Legend.ManualItems`, not on the panel itself.

ScottPlot 5's stock padding/margins are generous enough to leave a visible band of dead space around
a small chart like this - the `PixelPadding` values on `legendPanel` and `legendPanel.Legend`
(`Padding`, `Margin`, `SymbolWidth`, `SymbolPadding`, `InterItemPadding`) and the axis margins
(`plot.Axes.Margins(left:, right:, bottom:, top:)`) are deliberately trimmed below the library
defaults for that reason. `PixelPadding`'s two-argument constructor takes *per-side* pixels - the
`.Horizontal`/`.Vertical` properties it exposes are already doubled (both sides summed), which reads
as a doubling bug the first time you print one.

X-axis labels thin out for wide `daysLookback` windows instead of a fixed cadence like "every 7
days": `maxLabels = width / 40` and the label step is derived from `days.Count / maxLabels`, walking
backward from the most recent day so today always keeps a label. 40px/label is the same budget the
default 30-day window already renders comfortably at the UI's 1200px chart, so the 30-day case is
untouched (`step == 1`) and only wider windows thin out. Every day still gets a bar - only the axis
text gets sparser - so this doesn't cost any resolution the way capping `daysLookback` would.

## FileLength

`EmailDocument`/`ChiroEmailBatch` rows carry a `FileLength` populated at pdf-generation time from the blob's size
(see the `*PdfGenerationActivity` classes). Historical rows predating that column, plus a cohort of pre-2024-09-15
`EmailDocument` rows whose pdfs only ever lived in a since-retired `latex` blob container, were one-time backfilled
by `FileLengthBackfillBackgroundService`/`LatexPdfMigrationBackgroundService`, both since removed now that their
queries stopped matching anything - see git history if this needs revisiting.

## Access

The page accepts either the `Chiro` or `PdfStatus` role (`[RequiresToken(AccountRoles.Chiro,
AccountRoles.PdfStatus)]`) — `RequiresTokenAttribute`/`RequiresTokenFilter` treat multiple roles as
OR, not AND. `PdfStatusController` then re-checks role per section: `Chiro` sees the chiro
records/batches/pending-counts tables, `PdfStatus` additionally sees email documents. A
`Chiro`-only account passing the page-level check but failing a section's role check gets an empty
table with "No access." instead of the data, per the `User.IsInRole` checks already in
`Index.cshtml`.
