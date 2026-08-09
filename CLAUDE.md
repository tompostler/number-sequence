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

## Chiro dictation parsing

Full design notes: [`docs/chiro-dictation-parsing.md`](docs/chiro-dictation-parsing.md). Constraints that will
break things if forgotten:

- **The species form's choice arrays are the source of truth.** The model's schema is generated from them. Never
  hand-maintain a second copy of the vocabulary anywhere — that is the drift bug the whole design exists to avoid.
- **Clinical rules belong in `Resolve`, not the prompt.** Defaulting, mobilization exclusivity, and standing sites
  are rules, not judgements. The model has been observed getting all three wrong when asked.
- **One request per region, five regions.** The whole form in one request exceeds the API's compiled grammar limit.
  Keep a region's schema under roughly 6 KB.
- **Both chiro form handlers are named** (`OnPostParseAsync`, `OnPostSubmitAsync`) and both `<form>` tags carry an
  explicit `asp-page-handler`. There is no default handler on purpose: a form with no `action` posts to the current
  URL including its query string, so after a parse the stale `?handler=Parse` routed Submit into the parser.
- **`ChiroVocabulary` names are `nameof` of bound properties**, resolved by reflection in `ChiroForm.ApplyParse`.
  Renaming a bound property without updating the vocabulary fails at runtime, not at build.
