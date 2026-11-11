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

## Chiro records pending

"Chiro Records Pending" is the same shape as "Chiro Batch Pending Counts" but for `ChiroRecords`: unbounded by
`daysLookback`/`takeAmount`, so a record stuck failing pdf generation stays visible instead of aging out of the
windowed "Chiro Records" table above it. It isn't grouped into counts like the batch section, since each record is
individually actionable — its attempt count (`ChiroRecord.ProcessAttempt`) is the signal for whether
`ReprocessChiroRegularlyBackgroundService` has had a chance to retry it yet, versus whether it's still within the
original orchestration's own retry window. See [`docs/chiro-pdf-retry.md`](chiro-pdf-retry.md) for why retries work
this way.

Both chiro record lists project to `ChiroRecordRow` rather than loading `ChiroRecord`, so `InputJson` (the whole
filled-out form) never leaves the database for a page that only shows timestamps. That matters most here: this list
is the unbounded one, and it grows exactly when something is wrong. The chart below needs `InputJson` and projects to
just that and `RecordedAt`. The page's other queries are `AsNoTracking`, since the endpoint never saves.

## Account name

`ChiroRecords`, `EmailDocuments` and `ChiroEmailBatches` each carry the `AccountName` that submitted the work, and
every list on the page shows it (the pending counts don't, being grouped by recipient). It is copied forward at each
step rather than looked up: `ChiroController` stamps the record from the caller's identity,
`ChiroPdfGenerationActivity` copies the record's value onto the email document and every batch row it writes, and the
ledger pdf activities copy the invoice's or statement's own `AccountName`. Every row is self-describing, so the page
never has to join back through the chain, which matters for batch rows: their only link to a pdf is `AttachmentName`.

It is deliberately not a foreign key to `Accounts`. A cascade would delete the pdf history along with the account, and
a restrict would block the account's deletion on rows nobody wants to clean up first. A stale name on an old row is the
better failure. It is nullable for the same reason, and because older rows predate it.

The migration that added it (`AddPdfAccountName`) backfilled what was derivable. It filled records from
`Source = 'ui/{account}'`, chiro email documents from the instance id's `MakeHumanFriendly` RowId prefix, ledger email
documents from `{FriendlyId}_invoice`/`_statement`, and batches by `AttachmentName`. A step that matched more than one
account left the row null rather than guess. Records from the spreadsheet era, and everything downstream of them,
have no account to recover and stay null.

The backfill was first written as correlated `CROSS APPLY`s, one scan of the other table per row, and that ran past
the 30 second command timeout. Each match side is now grouped once and equality-joined. `MigrationService` also raises
the command timeout to 3 minutes on its own context, so a backfill isn't held to a limit meant for requests. The
app-wide timeout stays at the default, since a request that takes 30 seconds is a bug worth hearing about.

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

## Chiro forms per account chart

"Chiro Forms Per Account" is the same chart from the same endpoint, `GET pdfstatus/chart?groupBy=Account`
(`ChiroChartGrouping`), with each day's bar segmented by the submitting account instead of the clinic. It is a
parameter rather than a second endpoint because only the segment label differs. The day axis, stacking, legend and
label thinning above all apply unchanged, and a second copy of the plotting code would drift. `groupBy` defaults to
`Clinic`, so existing callers and the CLI's `--chart` keep their output (`--chart-group-by Account` for this one).

The account comes from the `ChiroRecords.AccountName` column (see [Account name](#account-name)), so this grouping
projects to `RecordedAt` and `AccountName` alone and never loads `InputJson`. Records with no account, from before the
column existed or that the backfill couldn't attribute, bucket into `(no account)`. That's the same treatment as
`(no clinic)`, so day totals match the clinic chart's.

Segment order is the one place the two charts differ. Clinics are a short fixed set and stay alphabetical. Accounts
are sorted by their window total, highest first, so the largest submitter sits at the bottom of every stack and leads
the legend. That chart exists to answer who submitted the most, and the order answers it. The placeholder bucket sorts
first in both.

## Manually backfilling a batch row

Occasionally a chiro record's pdf already exists (`ChiroRecord.ProcessedAt` is set, the pdf is in
blob storage, `EmailDocuments` has the row) but no `ChiroEmailBatches` row went out for a
destination it should have — e.g. a CC address that wasn't on the form at submission time. This is
rare enough that it isn't worth a UI action; a direct SQL insert is fine.

Pull `AttachmentName`, `FileLength`, and `CreatedDate` from the matching `EmailDocuments` row rather
than recomputing them, and copy `CreatedDate` through unchanged:

```sql
INSERT INTO dbo.ChiroEmailBatches (CcEmail, AttachmentName, FileLength, CreatedDate)
SELECT '<cc-email>', AttachmentName, FileLength, CreatedDate
FROM dbo.EmailDocuments
WHERE Id = '<email-document-id>';
```

Use `ClinicAbbreviation` instead of `CcEmail` for a clinic destination. Leave `Id` and `ProcessedAt`
out of the insert — `Id` gets its sequence default and `ProcessedAt` must stay `NULL` so
`ChiroBatchSendBackgroundService` picks the row up on its next scheduled run (10th/20th/30th, 10:10
AM).

**Copying `CreatedDate` is not optional.** `NsStorage.GetBlobClient(ChiroEmailBatch)` builds the
blob path as `{CreatedDate.Year}/{AttachmentName}.pdf`. `CreatedDate` defaults to
`SYSDATETIMEOFFSET()` at insert time, so a backfill done in a different calendar year than the pdf
was originally generated in would silently point at the wrong year's folder and fail to download
the blob when the batch sender runs.

## FileLength

`EmailDocument`/`ChiroEmailBatch` rows carry a `FileLength` populated at pdf-generation time from the blob's size
(see the `*PdfGenerationActivity` classes). Historical rows predating that column, plus a cohort of pre-2024-09-15
`EmailDocument` rows whose pdfs only ever lived in a since-retired `latex` blob container, were one-time backfilled
by `FileLengthBackfillBackgroundService`/`LatexPdfMigrationBackgroundService`, both since removed now that their
queries stopped matching anything - see git history if this needs revisiting.

## Access

There is no pdf status role. Each section is visible to the role whose feature produced its rows, and `Admin` sees
every section. The page accepts `Admin`, `Chiro` or `Ledger` (`RequiresTokenAttribute`/`RequiresTokenFilter` treat
multiple roles as OR). `PdfStatusController` re-checks per section and leaves a list empty when the viewer can't see it:

- **Chiro records, chiro email documents, batches, pending counts and both charts:** `Chiro`. Every chiro account sees
  all of them, the same data the chiro forms already share.
- **Ledger email documents:** `Ledger`, but only the documents whose `AccountName` is the viewer's own. Ledger data is
  partitioned by account everywhere else, and a status page shouldn't be the one place it isn't. `Admin` sees every
  account's, including rows with no `AccountName` that the account-name backfill couldn't attribute.
- **Unclassified email documents:** `Admin` only, and only rendered when there are any. They are rows whose `Kind` is
  neither `Chiro` nor `Ledger`, which only a creation site that forgot to set `Kind` produces, so the section surfaces
  the bug instead of the row silently going missing.

A section the viewer's roles don't reach isn't rendered at all; `IndexModel.ShowChiro`/`ShowLedger`/`ShowUnclassified`
mirror the controller's checks. A visible section that says "None." therefore means there are no rows, never no
access. That replaced an earlier "No access." placeholder per section, which left a ledger-only account looking at a
page that was mostly placeholders.

Email documents come back as three lists (`ChiroEmailDocuments`, `LedgerEmailDocuments`,
`UnclassifiedEmailDocuments`), one query each, rather than one list split on the page. `takeAmount` applies per
section, so a busy week of chiro pdfs can't push every ledger row out of a shared top-N.

It stays one page rather than a chiro page and a ledger page because the ledger side is a single table, and `Admin`,
the account watching the whole pipeline, is better served by one view. Splitting is worth revisiting if ledger grows
sections of its own.

`Kind` is stored rather than read off the id or subject. Both of those have changed shape over the years, so filtering
on them would tie access control to naming. The `AddEmailDocumentKind` migration used the id once, to classify existing
rows: anything containing `invoice` or `statement` is `Ledger` (that covers both the current `_invoice`/`_statement` and
the older `invoice-postler`/`statement-postler`), and everything else is `Chiro`. `Unknown` is the enum's zero value on
purpose: forgetting to set `Kind` lands a row in the admin-only unclassified section, never in front of the wrong role.

`PdfStatus` used to be a role that unlocked email documents. Its constant is gone, so it's no longer grantable. An
account that still has it stored shows it struck through on the admin accounts page, where unchecking it removes it.
