# Chiro entry timing

How long a chiro record took to fill in, kept separate from how long its pdf took to generate.
Each record now carries three times from the doctor's side of the form:

| Column | When | Set by |
| --- | --- | --- |
| `ChiroRecord.DataEnteredAt` | the species form was opened | the page, on `OnGet` |
| `ChiroRecord.DictationParsedAt` | the most recent dictation parse filled in the form, or null | the page, in `ApplyParse` |
| `ChiroRecord.RecordedAt` | the api recorded it | the database default |

The pdf status page derives three durations from those and `ProcessedAt`:

- **Entry** = `RecordedAt − DataEnteredAt`: the form being opened to it being submitted.
- **Review** = `RecordedAt − DictationParsedAt`: how long the doctor spent checking the parse.
  Blank with no parse.
- **Delay** = `ProcessedAt − RecordedAt` (or now, while pending): the pdf pipeline alone.

The page shows the durations but not the start and parse timestamps; those are the Entry and Review cells' hover text.
As columns, four timestamps beside the guid id came to about 1,350px, past the 1,320px of the widest Bootstrap container.
They're the recorded time less the durations anyway.
The contract still carries both, so the json has them, but the CLI table leaves them out for the same reason and has no hover to fall back on.

## Why the columns were already there but meant nothing

`DataEnteredAt` and `RecordedAt` were distinct back when records came in through Google Forms (see [chiro-pdf-retry.md](chiro-pdf-retry.md)): `DataEnteredAt` was the form's response time in the spreadsheet and `RecordedAt` was when the poller picked the row up.
When the in-app forms replaced that, the controller set `DataEnteredAt = UtcNow` in the same insert that defaults `RecordedAt`, so the two came out milliseconds apart.
`DataEnteredAt` is now "data entry began" again, with the form open as the start in place of the Google Form.

Delay used to be measured from `DataEnteredAt`, which covered the poller's lag as well as generation.
Once `DataEnteredAt` moved back to the form opening it would have added the doctor's fill time too, so it moved to `RecordedAt`.

## How the times get from the page to the record

The species pages are server-rendered with two separate forms, the transcript form (Parse) and the record form (Submit).
Each post carries only its own form's fields, so nothing persists between posts unless a form carries it.

- **Both times ride in hidden fields, in one partial (`_ChiroEntryTimes`) that both forms include.**
  The transcript form includes it directly; the record form gets it through `_ChiroFormIntake`.
  A time missing from either form is lost the first time that form posts.
  A parse post would otherwise come back with no start time, and a submit would never see the parse time.
- **The server stamps them, not browser script.**
  The page clock and the api clock are both the server's, so there's no client clock skew to reason about and nothing depends on script running.
- **The fields are written in the round-trip (`"O"`) format by hand, not with `asp-for`.**
  The input tag helper would format a `DateTimeOffset` in the current culture and drop the fraction of a second.
- **They travel to the api on `ChiroInput.DataEntryStartedAt` / `DictationParsedAt`**, set in `ChiroFormModel.SubmitRecordAsync`.
  Every species submits through it so a new species can't forget them.
  The controller moves both onto the record's own columns and clears them from the stored `InputJson`, so the times live in one place.
- **The controller drops implausible values** rather than recording them, because a hidden field can say anything: more than a minute in the future, or more than a day old.
  A start time it drops falls back to the receive time; a parse time it drops becomes null.
  The minute of future allowance is for App Service serving the page and the api from different instances.
  The day cap is so a tab left open overnight doesn't skew the numbers.
  A caller that sends neither, such as an older client, gets the old behavior.

## Decisions

- **Page load and parse, both.** Page load alone overstates entry for a tab opened early and filled in later.
  Parse alone has nothing for a form filled in by hand.
  With both, Entry is the upper bound and Review is the part spent checking the parse.
- **The most recent parse, not the first.** A re-parse replaces the form's contents, so review effectively restarts from it.
- **Opening a form fresh restarts the clock.** The start comes from `OnGet`, so navigating back to the species page starts over.
  A failed submit or a parse re-render keeps it, since those are posts carrying the hidden field.

## Reading old rows

- In-app records from before this change show an Entry of about zero and no Review.
  Nothing recorded the start then.
  Their Entry is milliseconds, and it can even come out slightly negative: `RecordedAt` is the database's clock and `DataEnteredAt` the app's.
  The page clamps it at zero.
  A window holding nothing but these rows is also why the format picker needs a short format for spans under a second; it used to fall back to the full `dd.hh:mm:ss.fff`.
- Google Forms records show the poller's lag as Entry, not fill time.
- Records copied from `PdfTemplateSpreadsheetRows` have `RecordedAt = ProcessedAt`, so their Delay reads zero.
