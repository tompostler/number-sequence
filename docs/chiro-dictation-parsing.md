# Chiro dictation parsing

Turns a dictation transcript into a prefilled canine or equine chiro form, for a human to check and submit.

Scribenote records the visit and produces the transcript. The chiro pastes it into the textarea on
`/ui/chiro/{species}`, presses Parse, and the form comes back filled in with a short list of things to check.
Nothing is recorded until they press Submit.

Scribenote also produces a structured version of the exam, with its own numbered option lists. **That output is
deliberately not used.** Its option list was a hand-maintained copy of this form and had drifted in six places, so
mapping through it was lossy: it emitted sided posterior levels and sided SCP options the form cannot express, and
a forelimb hypermobility option that does not exist clinically. Going straight from the transcript removed four of
those six mismatches outright, and the other two turned out to be Scribenote template bugs rather than gaps here.

## Why it is shaped this way

**The form's own arrays are the source of truth.** `CanineModel` and `EquineModel` hold the choice arrays the
checkboxes render from. The model's JSON schema is generated from those same arrays at request time, so the values
it can return and the values the form can render cannot diverge. This is the whole reason the Scribenote hop was
removed; reintroducing any hand-kept second copy of the vocabulary brings the drift back.

**Enums are the guardrail, prose is not.** Every selectable value is an enum member in the schema. The model
physically cannot return an option that is not on the form. The prompt only carries things a schema cannot express:
that the input is speech-to-text, what counts as a finding, and what to do when the words fit nothing.

**Everything the server can decide, the server decides.** Clinical rules are applied in `Resolve`, not asked of the
model. They are rules, not judgements, and a rule the model cannot forget is worth more than the tokens spent
restating it. This was learned the hard way twice: Scribenote's equivalent rule fabricated findings on all 13 ribs,
and an early version of our prompt had the model inferring mobilization across a run-on list of levels that had in
fact been adjusted.

**The form is parsed in five regions, concurrently.** Not a tuning choice — one request for the whole form compiles
a grammar the API rejects:

> The compiled grammar is too large, which would cause performance issues.

The whole-form schema was 11,563 chars. Per region it is 2.7–4.7 KB, with forelimbs the largest. Keep a region
under roughly 6 KB. The happy side effect is that a parse takes about as long as the slowest region rather than the
sum of all of them.

**Flags are transient.** They are the human's review list, never persisted, never in the PDF. They exist so that
checking a parse means reading three or four items rather than re-reading ninety checkboxes against the transcript.
A flag is a question for the reviewer, not an explanation of the model's work — an early version produced fourteen
flags for a 533-character transcript, most of them narrating correct decisions.

## Clinical semantics

Domain knowledge, not derivable from the code. Confirmed with the doctor.

- **Mobilization means the site was assessed and needed no adjustment.** It is therefore what an unmentioned
  question means, and it is mutually exclusive with a finding for the same site — if a level was adjusted, it was
  not left alone.
- **Some sites are assessed at every visit** regardless of what was dictated. On the rear limb these are the
  coxofemoral (both cranial and caudal, unless the dictation names a direction) and the phalanges (always, whatever
  the digit findings say). Declared as `StandingSites` on the group.
- **A question covering several joints is not all-or-nothing.** Adjusting the digits says nothing about whether the
  hip was looked at, so the mutual-exclusion rule only applies where the bare word `mobilization` is the question's
  only mobilization option.
- **An intended-but-not-performed adjustment is not a finding.** Resisted, deferred, or not tolerated goes in the
  notes and must never tick a box.
- **The transcript is speech-to-text and contains recognition errors.** `cairo` is `chiro`; `post` is `posterior`;
  "on the front" means the forelimb. A side stated once carries across the items following it in the same phrase.

## Where things live

| | |
|---|---|
| `Utilities/ChiroVocabulary.cs` | Describes a species form: regions, questions, choices, notes, standing sites, glossary |
| `Utilities/ChiroSchemaBuilder.cs` | Generates one region's JSON schema from that description |
| `Services/ChiroDictationParser.cs` | The five parallel requests, the prompt, revalidation, the clinical rules, merge |
| `Pages/UI/Chiro/{Canine,Equine}.cshtml.cs` | Choice arrays, the `Vocabulary` built from them, both post handlers |
| `Pages/UI/Chiro/ChiroForm.cs` | `ApplyParse`, which writes a draft onto a page model by property name |
| `Pages/Shared/_ChiroTranscript.cshtml` | Textarea, Parse button, spinner, flag list |

Every name in a `ChiroVocabulary` is a `nameof` of the bound property it fills. `ApplyParse` resolves them by
reflection, which is what keeps the parser species-agnostic — a new species needs a `Vocabulary` and a handler,
nothing else. The compiler cannot see that link, so a renamed bound property will fail at runtime, not build.

## Configuration

```jsonc
"Claude": { "ApiKey": "sk-ant-..." },
"Email": {
  // Breaking change from the old {"ABBV": "email"} shape. The spelled out name is what lets a dictation
  // mentioning the clinic resolve back to its abbreviation. Only names and keys reach the API, never emails.
  "ChiroBatchMap": "{\"ABBV\":{\"name\":\"Abbreviation Animal Clinic\",\"email\":\"...\"}}"
}
```

## Tuning

Model and effort are constants at the top of `ChiroDictationParser`. Currently `claude-sonnet-5` at `Medium`.

Cost is negligible — roughly a cent or two per parse — so latency is the only reason to change anything. Read it
off the logs, which report per region and overall:

```
[forelimbs] requesting. System prompt 2402 chars, schema 4729 chars, 12 groups, 0 grids, 2 notes.
[forelimbs] parsed in 3140ms. Stop reason end_turn. Input 38, cache write 0, cache read 1893, output 141.
Parsed Canine dictation across 5 regions in 4210ms total.
```

Total is the slowest region. If one region dominates, splitting that region beats changing model — forelimbs is the
obvious candidate and divides cleanly into left and right.

`claude-haiku-4-5` is the cheap option but is **not** a one-constant change: `effort` is unsupported there and will
return a 400, so `OutputConfig.Effort` has to come out, and Haiku has no thinking on by default.

When sweeping, do not judge by the flags — they only show what the model noticed was ambiguous. Run the same
transcripts through each config and diff the resulting checkboxes. Test negation first; it is the highest
consequence failure, because getting it wrong ticks something that did not happen.

## Open questions

- **Canine rear limb stops at digit 4.** A dictation saying "digits 4 and 5 on the back" has nowhere to put digit 5.
  Real dewclaw, or a transcription slip?
- **Equine has never been run against a real transcript.** It shares all the machinery, but its vocabulary
  (DALMA, Pastern, proximal Sesamoid, intertransverse) has not been exercised.
- **`ChiroVocabulary.Glossary` is empty on both species.** It is the place for species-specific terminology as it
  turns up; the section is omitted from the prompt while empty.
- **Notes cross-contamination.** Each region sees the whole transcript, and a phrase with no home in that region's
  questions used to land in its notes — coxofemoral text in `PelvicNotes`, tail traction in `SacrumNotes`. Addressed
  by telling each region its notes fields are not a catch-all. Worth watching; if it recurs, the next lever is not
  showing each region the full transcript, at the cost of the laterality context it needs.
- **Transcripts go to Anthropic**, patient and owner names included, and are logged at Information, which means
  Application Insights in production. Scribenote already receives the audio, so it is not a new category of
  exposure, but the logging is a deliberate choice and can be gated to development.
