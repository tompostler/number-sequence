# Working in this repo

## Build and test

```sh
dotnet build src/number-sequence/number-sequence.csproj
dotnet test src/UnitTests/UnitTests.csproj
```

There is no solution file — build projects individually. Projects: `number-sequence` (web), `Client`, `Contracts`,
`Tool`, `UnitTests`, `IntTests`. Central package management, so package versions go in `Directory.Packages.props`
and the `PackageReference` in the csproj carries no version.

**Never run `dotnet msbuild -t:CoreCompile` to work around a locked output file.** It compiles without the
resource-preparation targets, leaving `obj` with an assembly that has no embedded resources. The next incremental
build sees that as up to date and copies it to `bin`, and the app then dies at startup on
`FontManager.RegisterFont` with a null stream. If the exe is locked because the app is running, ask for it to be
stopped. Recovery is `dotnet build --no-incremental`.

## Preferences

- **Running the code beats maintaining unit tests here.** Tests that have to be rewritten every time a shape
  changes are not worth their upkeep. Prefer a quick real run over a test suite.
- **Log at Information.** Volumes are small; a level nobody has switched on is not useful. Log the inputs, timings,
  sizes, and anything not reconstructible from source. Do not log things that are generated from source, such as a
  prompt or a schema — sizes are enough.

## Documenting a feature

**A feature is not done until its docs are written.** Do it in the same turn as the code, before reporting done —
not as a follow-up offer. Two places, and they hold different things:

- **`docs/<feature>.md`** — the full design notes. Why it is shaped this way, what the constraints were, what was
  tried and rejected, and the flow through the pieces. New feature gets a new file; a change to an existing feature
  edits that feature's file. [`docs/chiro-dictation-parsing.md`](docs/chiro-dictation-parsing.md) is the pattern.
- **This file** — a one-line pointer to the design notes, followed only by the constraints that will break things
  if forgotten. Keep it short; everything here is read on every single session, so it competes with everything else.

Write what is not reconstructible from the source: decisions, limits found the hard way, the coupling between two
files that nothing in either file mentions. Do not restate class structure, method lists, or anything a reader gets
faster by opening the file. `README.md` is build and release badges only — it is not the place for any of this.

## Deployment

App settings live in [`deployment/templates/AppServiceSettings.json`](deployment/templates/AppServiceSettings.json).
That resource replaces the **whole** appsettings collection on every deploy, so a setting added in the portal is
gone at the next one. Add it to the template.

`LANG` is set there deliberately. The plan is Linux, and .NET on a host with no `LANG` falls back to the invariant
culture, which renders `.ToString("C")` as `¤12.34` — the ledger UI is full of those.

## Chiro dictation parsing

Full design notes: [`docs/chiro-dictation-parsing.md`](docs/chiro-dictation-parsing.md). Constraints that will
break things if forgotten:

- **The species form's choice arrays are the source of truth.** The model's schema is generated from them. Never
  hand-maintain a second copy of the vocabulary anywhere — that is the drift bug the whole design exists to avoid.
- **Canine and feline share `SmallAnimalChiroFormModel` and `_ChiroSmallAnimalForm.cshtml`** because their forms
  are identical. Equine is separate and still needs every rule change applied to it by hand. Do not generalize the
  shared model to cover a species that actually differs.
- **Clinical rules belong in `Resolve`, not the prompt.** Defaulting, mobilization exclusivity, and standing sites
  are rules, not judgements. The model has been observed getting all three wrong when asked.
- **One request per region, six regions.** The whole form in one request exceeds the API's compiled grammar limit.
  Keep a region's schema under roughly 6 KB.
- **Both chiro form handlers are named** (`OnPostParseAsync`, `OnPostSubmitAsync`) and both `<form>` tags carry an
  explicit `asp-page-handler`. There is no default handler on purpose: a form with no `action` posts to the current
  URL including its query string, so after a parse the stale `?handler=Parse` routed Submit into the parser.
- **The pdf filename comes from `ChiroDocumentNaming`**, used by both the generation activity and the submitting
  page, which shows the doctor the name before the pdf exists. Two copies would be two filenames.
- **`ChiroVocabulary` names are `nameof` of bound properties**, resolved by reflection in `ChiroForm.ApplyParse`.
  Renaming a bound property without updating the vocabulary fails at runtime, not at build.
