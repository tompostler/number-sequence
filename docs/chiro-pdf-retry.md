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

**Retrying is a new orchestration, not a longer one.** `ReprocessChiroRegularlyBackgroundService` runs every 30
minutes (long enough that the original orchestration's own ~20-minute retry window has already resolved one way or
the other) and creates a fresh orchestration for every `ChiroRecord` with `ProcessedAt == null` and `InputJson`
set. This is the same shape as `ReprocessLedgerRegularlyBackgroundService`, which already does this for recurring
invoices — the difference is this one is driven by failure, not by a recurrence schedule.

**`InputJson == null` is excluded deliberately, not defensively.** The google sheet ingestion services record a
`ChiroRecord` with no `InputJson` when the row's submitter isn't on the template's allowed list, specifically so
the row is never re-read as new on the next poll. That record is meant to stay unprocessed forever. Without this
filter the reprocessing service would treat every rejected submission as a stuck straggler and spin on it every 30
minutes forever.

## Orchestration instance id collisions

The orchestration instance id was deterministic from the row id alone (`{rowId.MakeHumanFriendly()}_{templateId}`),
computed identically at every creation site: `ChiroController`, both google sheet ingestion services, and now
`ReprocessChiroRegularlyBackgroundService`. Calling `CreateOrchestrationInstanceAsync` again with that same id for
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

`PdfStatusController` exposes `ChiroRecordsPending` — every unprocessed, non-rejected `ChiroRecord`, unbounded by
`daysLookback`/`takeAmount` — the same reasoning as the existing `ChiroBatchPendingCounts` section: a straggler
old enough to fall out of the windowed `ChiroRecords` table should still be visible. Unlike the batch pending
section it isn't grouped/counted, since each record is individually actionable (its `ProcessAttempt` tells you
whether reprocessing has even had a chance to run yet).
