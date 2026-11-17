# Counts

A count is a named `ulong` per account, incremented by one or by an amount.
Each increment appends a `CountEvent` holding the amount and the count's `Value` after it, which the events list and the chart read.
It is exposed in the api (`/counts`), the Client (`CountOperations`), the Tool (`count`) and `/ui/counts`.

## Event history

`CountEvent.Value` is a snapshot of the running total, not something derived at read time.
That is what lets the oldest events be dropped (`OverflowDropsOldestEvents`) without losing the value of the ones that remain.
The tier limit on events per count (`TierLimits.CountEventsPerCount`) is enforced at increment time: either the increment is rejected or the oldest events are dropped to make room.

## Back-dating an increment

An increment can carry an earlier time (`?at=` on both increment routes, `--at` in the Tool, the date field beside "+ amount" on the detail page).
The case it exists for is recording something the morning after, like a deployment that succeeded overnight.

`EventDate` is when the increment happened, and `CreatedDate` stays when it was recorded, so a back-dated event is still visible as one.
The detail page shows the recorded time only on events where the two differ by more than a minute.
Filtering, ordering, the chart and overflow dropping all go by `EventDate`.
The migration that added it filled existing rows from `CreatedDate`.

**A back-dated event may not be earlier than the count's latest event; the api answers 409.**
Inserting one in the middle of the history would leave every later event's `Value` short by its amount, and the chart would sawtooth.
The alternative was to add the amount to every later event's `Value` in the same save.
It was rejected as more machinery than the use case needs: back-dating is for catching up on what was missed, not reordering history.
Because of this rule, `EventDate` order and `Id` order agree, and `Id` breaks ties.

A time in the future is a 400.
There is no tolerance for clock skew, on purpose: an event even a minute in the future would make every ordinary increment in that minute look earlier than the latest event.
Ordinary increments don't run the check at all and always record now.

The time is normalized to UTC on the server, because the Tool parses a bare time with the local offset and the page labels every time as UTC.
The page's `datetime-local` input carries no zone, so its script converts the browser's local time to an ISO UTC string in a hidden `At` field on submit.
Only "+ amount" takes a time; the "+1" button stays one tap for the common case, and an amount of 1 with a time covers the rest.

Editing the time of an event after it is recorded was considered and left out.
It would need the running-total fix-up above, an endpoint, and an edit UI per event.
