# Chiro pdf generation retry

Gives a chiro record's pdf generation effectively unlimited retries instead of the fixed six the durable
orchestration itself allows, and surfaces the stragglers on `/ui/pdf-status` so a systemic failure gets noticed
instead of silently sitting unprocessed.

## Why it is shaped this way

**The orchestration's own retry is bounded on purpose, so it stays bounded.**
`ChiroGenerationOrchestrator` schedules `ChiroPdfGenerationActivity` with
`DefaultExponentialRetryOptions` — 6 attempts, exponential backoff, capped at 20 minutes total. That's tuned for
transient failures within a single request (a blob storage blip, a momentary SQL timeout) and is shared with the
ledger invoice/statement orchestrators. Widening it to retry forever inside one orchestration execution would mean
a single stuck instance parked in the durable task worker indefinitely, which fights the "single active
orchestration" model (`MaxActiveOrchestrations = 1` in `DurableOrchestrationWorkerBackgroundService`) — nothing
else would get generated while it spun.

**Retrying is a new orchestration, not a longer one.** `ReprocessChiroRegularlyBackgroundService` runs every hour
and creates a fresh orchestration for every `ChiroRecord` with `ProcessedAt == null`, `InputJson` set, and
`RecordedAt` more than an hour old. The hour on `RecordedAt` is what keeps it clear of the original orchestration's
own ~20-minute retry window, which has definitely resolved one way or the other by then. This is the same shape as `ReprocessLedgerRegularlyBackgroundService`, which already does this for recurring
invoices — the difference is this one is driven by failure, not by a recurrence schedule.

There is no `InputJson != null` filter. Every unprocessed record has one; see
[Google Sheets ingestion (removed)](#google-sheets-ingestion-removed) for the rows that don't.

## Orchestration instance id collisions

The orchestration instance id was deterministic from the row id alone (`{rowId.MakeHumanFriendly()}_{templateId}`),
computed identically at every creation site: `ChiroController`, the since-removed google sheet ingestion services,
and now `ReprocessChiroRegularlyBackgroundService`. Calling `CreateOrchestrationInstanceAsync` again with that same id for
a retry collides with the id the failed attempt already used.

`ChiroRecord.ProcessAttempt` (bumped before each retry) is folded into the id via
`ChiroOrchestrationNaming.InstanceId(rowId, templateId, processAttempt)`, the same way `Invoice.FriendlyId` folds
in `ProccessAttempt` for the ledger orchestrations. Every creation site — including the very first attempt, which
starts at `ProcessAttempt == 0` — goes through this one helper so the id scheme can't drift between call sites the
way the old duplicated `$"{id.MakeHumanFriendly()}_{template.Id}"` string could have.

The reprocessing service doesn't have the `PdfTemplate` row in hand the way the creation sites do (it only has the
`ChiroRecord`), so it derives the equivalent id via `ChiroSpeciesDefinition.Get(chiroInput.Species).TemplateId`
instead of a `PdfTemplate` lookup.

## No error message column

`ChiroRecord` intentionally has no `LastError` column. The attempt count plus `RowId` is enough to find the
failure in Application Insights (`ChiroPdfGenerationActivity`'s operation is correlated to the orchestration
instance id), and a column that duplicates telemetry is one more thing to keep in sync with no reader who'd use it
from the UI instead of App Insights.

## Pending records on the status page

`PdfStatusController` exposes `ChiroRecordsPending` — every unprocessed `ChiroRecord`, unbounded by
`daysLookback`/`takeAmount` — the same reasoning as the existing `ChiroBatchPendingCounts` section: a straggler
old enough to fall out of the windowed `ChiroRecords` table should still be visible. Unlike the batch pending
section it isn't grouped/counted, since each record is individually actionable (its `ProcessAttempt` tells you
whether reprocessing has even had a chance to run yet).

## Google Sheets ingestion (removed)

Until September 2026, canine and equine records also came in through two Google Forms. Two background services
polled the forms' response spreadsheets through a Google service account and recorded one new row per poll. The
in-app forms replaced that path, and it was removed once both Google Forms were closed and fully ingested. What it
left behind in `ChiroRecords`:

- **`Source` holds a spreadsheet id** on records from that path, instead of `ui/{submitter}`.
- **Processed records with no `InputJson`** predate `ChiroRecord` itself. They were copied from the older
  `PdfTemplateSpreadsheetRows` table, which only tracked rows and never stored their contents. They are history,
  which is why `InputJson` stays nullable and the per-clinic chart still guards for null.
- **Unprocessed records with no `InputJson`** were rows from a submitter not on the template's allow-list,
  recorded only so the poller wouldn't read them as new again. The `RemoveGoogleSheetIngestion` migration deleted
  them, which is what lets the reprocessing and pending queries treat every unprocessed record as real.
