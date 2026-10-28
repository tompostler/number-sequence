# Working in this repo

## Build and test

```sh
dotnet build src/number-sequence/number-sequence.csproj
dotnet test src/UnitTests/UnitTests.csproj
```

The solution is `src/number-sequence.slnx`, which is what CI restores and builds. Building a single project, as
above, is faster while iterating. Projects: `number-sequence` (web), `Client`, `Contracts`, `Tool`, `UnitTests`,
`IntTests`, `_repo`. Central package management, so package versions go in `Directory.Packages.props`
and the `PackageReference` in the csproj carries no version.

**Never run `dotnet msbuild -t:CoreCompile` to work around a locked output file.** It compiles without the
resource-preparation targets, leaving `obj` with an assembly that has no embedded resources. The next incremental
build sees that as up to date and copies it to `bin`, and the app then dies at startup on
`FontManager.RegisterFont` with a null stream. If the exe is locked because the app is running, ask for it to be
stopped. Recovery is `dotnet build --no-incremental`.

The user often has `dotnet watch` running in another tab while iterating, which holds `number-sequence.exe`
locked. A validation `dotnet build` in that situation fails at the final copy step (`MSB3027`/`MSB3021`) after
compilation has already succeeded — that failure is benign and doesn't mean the change is broken. Real compile
errors (`CS####`) show up before that copy step regardless of the lock, so read for those rather than treating
every red "Build FAILED" as a problem to chase.

## Preferences

- **Running the code beats maintaining unit tests here** — but the user does that running, not Claude. Don't
  start the app (`dotnet run`, IIS Express, etc.) or drive it in a browser to validate a change; the user runs and
  validates themselves, often via a `dotnet watch` tab already open. Confirm with a build instead, reading for
  `CS####` errors rather than treating a locked-exe copy failure as a problem — see the note above.
- Tests that have to be rewritten every time a shape changes are not worth their upkeep. Prefer a quick real run
  over a test suite for verifying behavior, understanding that the user is the one doing that run.
- **JSON goes through `Utilities.RelaxedJson.Options` everywhere in the web project, reads as well as writes.** It
  uses the relaxed encoder (a bare call escapes non-ascii as `\uXXXX`, which is what ends up in the db) and writes
  enums by name. A bare `Deserialize` can't parse a named enum, so a reader without these options throws on any stored
  json containing one. MVC (`Startup`) and the Client are configured the same way. Html escaping is razor's job at
  display time, so never `Html.Raw` a stored value.
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

`WEBSITE_RUN_FROM_PACKAGE` is set to `1` so the deploy's zip is mounted read-only and swapped atomically instead of
extracted file-by-file — code lands all at once instead of whenever App Service gets around to each file. This is
the local-zip variant, not the run-from-URL variant: no storage account or SAS involved, since the existing
`AzureRmWebAppDeployment@4` zip-deploy step already ships the zip to Kudu.

## Static assets

Full design notes: [`docs/static-assets.md`](docs/static-assets.md). Constraints that will break things if forgotten:

- **Bootstrap is vendored under `wwwroot/lib/bootstrap` and served by `MapStaticAssets`; don't reintroduce a CDN
  link.** Update it from the npm tarball, checked against the registry's integrity hash, as the notes describe.
- **Reference anything in `wwwroot` as `~/path` with `asp-append-version="true"`.** That is what produces the
  fingerprinted, `immutable` url. A literal `/lib/...` href gets the plain route, which is `no-cache` and costs a
  revalidation on every page load. A file fetched by script rather than a tag gets its url from `StaticAssetUrls.For`.
- **`barcode-detector` and `zxing-wasm` are updated together**, zxing-wasm at exactly the version barcode-detector
  depends on. The bundle embeds zxing-wasm's glue code, and a mismatched `.wasm` fails at runtime.

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
- **The repo is public: never copy a real transcript into the prompt, docs, or tests.** Finding-run fragments
  ("L6 left L5 left") are fine; patient, owner and clinic names, visit dates and history narrative are not.
- **`ChiroVocabulary` names are `nameof` of bound properties**, resolved by reflection in `ChiroForm.ApplyParse`.
  Renaming a bound property without updating the vocabulary fails at runtime, not at build.

## PDF status page

Full design notes: [`docs/pdf-status.md`](docs/pdf-status.md). Constraint that will break things if forgotten:

- **The pending-count section deliberately ignores `daysLookback`/`takeAmount`.** It exists specifically to
  surface stragglers the windowed list views hide.
- **The per-clinic chart counts `ChiroRecords`, not `ChiroEmailBatches`.** `ChiroEmailBatches` has an indexed
  clinic column and is cheaper to query, but it's a post-processing artifact (duplicate CC rows, drops
  clinic-less records). `ChiroRecords` requires deserializing `InputJson` per row but is the true "form
  filled out" count. Don't swap the source without renaming the section.
- **Backfilling a missed `ChiroEmailBatches` row by hand must copy `CreatedDate` from the matching
  `EmailDocuments` row**, not let it default to now — the blob path is keyed by `CreatedDate.Year`, so a
  default from a different calendar year than the original pdf points at the wrong folder.

## Chiro pdf generation retry

Full design notes: [`docs/chiro-pdf-retry.md`](docs/chiro-pdf-retry.md). Constraint that will break things if
forgotten:

- **The orchestration instance id always goes through `ChiroOrchestrationNaming.InstanceId`**, at every creation
  site (`ChiroController`, `ReprocessChiroRegularlyBackgroundService`). It folds in `ChiroRecord.ProcessAttempt`
  so a retry's id doesn't collide with the failed attempt it's replacing — hand-building the id anywhere brings
  that collision back.

## Account administration

Full design notes: [`docs/account-admin.md`](docs/account-admin.md). Constraints that will break things if forgotten:

- **Any write to `Account.Tier` or `Account.Roles` must evict that account's tokens from `IMemoryCache`.** The
  validated principal is cached by raw token value with a sliding expiration, so an account in active use otherwise
  keeps its old roles until its token expires. Go through `AccountsController.UpdateAccountAsync` rather than saving
  an account elsewhere.
- **`AccountRoles.All` is reflected from the `const string` fields.** A non-role constant added to that class becomes
  grantable.

## Library

Full design notes: [`docs/library.md`](docs/library.md). Constraints that will break things if forgotten:

- **Every library-scoped query goes through the access helper, never `AccountName == User.Identity.Name`.** The Ledger
  pattern copied here silently locks out every account a library is shared with.
- **Every barcode written or searched for goes through `LibraryBarcode.TryParse`**, which brings ISBN-10 and UPC-A to
  13 digits. A path that skips it stores a form the others don't match, and lookups and scans quietly miss.
- **Copies are found by a code only through `LibraryBarcode.MatchesCopy`**, which checks both `Barcode` and `Isbn`. A
  query on `Barcode` alone misses every book whose printed barcode is a UPC.
- **Event times come from the row that owns the date** (`LibraryCopy.AcquiredDate`, `LibraryLoan.LoanedDate` and
  `ReturnedDate`) through one helper, and are never stored on the event. Events are append-only, so a copied date goes
  stale the moment a purchase date is corrected. The helper's clamp keeps each copy's events in recorded order, which
  the gource export needs.
- **API responses are whole entities, not hand-mapped partial ones.** A field left out of a projection looks exactly
  like an empty one to every client. Use `Include`; item search is the one documented exception.
- **Item and copy updates replace every editable field.** A page that sends its bound form wipes whatever the form
  doesn't carry, such as `ExternalIds` and `Attributes`. Apply the form onto the fetched entity and send that.
- **Covers only ever pass through `LibraryCovers`**, which fetches from lookup provider hosts alone (checked on every
  redirect) and re-encodes every image. Fetching a cover url anywhere else is an open fetch-any-url endpoint, and serving
  uploaded or fetched bytes as-is lets an html or svg file be served from this site.
- **`LibraryExternalIds` keys are how the add page finds an existing item.** Renaming one silently stops matching items
  saved under the old key.
- **A lookup token needs Key Vault, the `tompostler` variable group, `deploy-code.yml`, `AppServiceSettings.json` and
  `devinit.ps1`.** A secret missing from the variable group reaches the app as the literal `$(name)`, which counts as no
  token, so forgetting it leaves that lookup quietly off rather than failing the deploy.
- **The namespace and pages folder are `Libraries`, not `Library`**, because the root entity is `Library` and a type named
  like its own namespace breaks name resolution (CS0118).
