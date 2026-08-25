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

## FileLength backfill

`FileLengthBackfillBackgroundService` fills in `FileLength` on `EmailDocument`/`ChiroEmailBatch` rows that predate
the column. It treats `FileLength == 0` as "needs backfilling" rather than tracking a separate flag — safe because
a real generated pdf is never zero bytes, and both models default the column to `0`. It reads the size straight off
the blob already in storage (`NsStorage.GetBlobClient`) instead of recomputing anything.

It's best-effort: a blob that 404s (e.g. aged out of retention) is logged and left at `0`, which means it's
retried on every run indefinitely. That's intentional given the low volume here rather than worth the complexity of
a separate "gave up" marker - see [`SqlSynchronizedBackgroundService`](../src/number-sequence/Services/Background/SqlSynchronizedBackgroundService.cs)
for the run-once-per-cron-tick machinery this and the other background services share.

## Latex pdf migration

The `FileLength` backfill surfaced a second, older gap: `EmailDocument` rows processed before the 2024-09-15
QuestPDF cutover (commit `7163fe1`) were never copied into the `pdf` blob container in the first place. Before
that date, `EmailPdfForLatexBackgroundService` (removed in `70da9ae`) attached the pdf straight out of the retired
`latex` container's `{id}/output/{id}.pdf` path and never wrote a copy into `pdf`, so `FileLengthBackfillBackgroundService`
correctly gets a 404 for these and can never fill them in on its own.

`LatexPdfMigrationBackgroundService` closes that gap: for `EmailDocument` rows with `FileLength == 0`, a set
`ProcessedAt`, and `CreatedDate` before the cutover, it does a same-account `SyncCopyFromUriAsync` from
`NsStorage.GetLegacyLatexPdfBlobClient` into the normal `pdf` location, then reads the copy's size back for
`FileLength`. The cutover date isn't a performance filter — it's load-bearing: `ChiroEmailBatch` postdates the
QuestPDF switch entirely, so it's never a candidate and isn't queried here, and any *post*-cutover row with
`FileLength == 0` is a real bug that this service must not paper over by matching it.

This is a one-time migration for a fixed, closed set of historical rows, not evergreen infrastructure like the
`FileLength` backfill above — it, `NsStorage.GetLegacyLatexPdfBlobClient`, and its `Startup.cs` registration should
be deleted together (a single revert) once its query stops matching anything.

## Access

The page accepts either the `Chiro` or `PdfStatus` role (`[RequiresToken(AccountRoles.Chiro,
AccountRoles.PdfStatus)]`) — `RequiresTokenAttribute`/`RequiresTokenFilter` treat multiple roles as
OR, not AND. `PdfStatusController` then re-checks role per section: `Chiro` sees the chiro
records/batches/pending-counts tables, `PdfStatus` additionally sees email documents. A
`Chiro`-only account passing the page-level check but failing a section's role check gets an empty
table with "No access." instead of the data, per the `User.IsInRole` checks already in
`Index.cshtml`.
