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
