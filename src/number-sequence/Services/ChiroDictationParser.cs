using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Options;
using number_sequence.Utilities;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace number_sequence.Services
{
    /// <summary>
    /// Maps a dictation transcript onto a species form's own vocabulary.
    /// <para>
    /// This only ever produces a draft. Nothing it returns is submitted without a human looking at it, which is why
    /// it is allowed to guess at all: a guess plus a flag is useful, and a guess on its own would not be.
    /// </para>
    /// <para>
    /// The form is parsed a region at a time, concurrently. The whole form in one request compiles a grammar the api
    /// rejects as too large, and the split has the happy side effect of making the round trip about as long as the
    /// slowest region rather than the sum of all of them.
    /// </para>
    /// </summary>
    public sealed class ChiroDictationParser
    {
        // Tune these against real transcripts. The per region and overall log lines report the model, the effort,
        // the elapsed time, and the output token count together, which is what makes a sweep readable.
        // A region is a small, bounded extraction with the legal values already fixed by the schema, so this starts
        // well below the top of the range; claude-haiku-4-5 at low is the next step down if this holds up.
        private const string Model = "claude-sonnet-5";
        private static readonly Effort ParseEffort = Effort.Medium;

        private readonly AnthropicClient client;
        private readonly Options.Email emailOptions;
        private readonly ILogger<ChiroDictationParser> logger;

        public ChiroDictationParser(
            IOptions<Options.Claude> claudeOptions,
            IOptions<Options.Email> emailOptions,
            ILogger<ChiroDictationParser> logger)
        {
            this.client = new AnthropicClient { ApiKey = claudeOptions.Value.ApiKey };
            this.emailOptions = emailOptions.Value;
            this.logger = logger;
        }

        public async Task<ChiroParseResult> ParseAsync(ChiroVocabulary vocabulary, string transcript, CancellationToken cancellationToken)
        {
            Dictionary<string, Options.ChiroClinic> clinics = this.emailOptions.ChiroBatchMapParsed;

            this.logger.LogInformation(
                $"Requesting {vocabulary.Species} dictation parse across {vocabulary.Regions.Count} regions. "
                + $"Model {Model}, effort {ParseEffort}, transcript {transcript.Length} chars.");
            this.logger.LogInformation($"Request transcript:{Environment.NewLine}{transcript}");

            Stopwatch overall = Stopwatch.StartNew();

            // The intake fields belong to exactly one region so that two requests cannot answer the patient name
            // differently. The first region is the natural home: a dictation states who it is about before it starts
            // listing findings.
            RegionParse[] parses = await Task.WhenAll(
                vocabulary.Regions.Select((region, index) =>
                    this.ParseRegionAsync(vocabulary, region, index == 0, transcript, clinics, cancellationToken)));

            overall.Stop();
            this.logger.LogInformation($"Parsed {vocabulary.Species} dictation across {parses.Length} regions in {overall.ElapsedMilliseconds}ms total.");

            return Merge(parses);
        }

        private async Task<RegionParse> ParseRegionAsync(
            ChiroVocabulary vocabulary,
            ChiroVocabularyRegion region,
            bool includeIntake,
            string transcript,
            Dictionary<string, Options.ChiroClinic> clinics,
            CancellationToken cancellationToken)
        {
            Dictionary<string, JsonElement> schema = ChiroSchemaBuilder.Build(region, clinics.Keys, includeIntake);
            string systemPrompt = BuildSystemPrompt(vocabulary, region, includeIntake, clinics);
            string schemaJson = JsonSerializer.Serialize(schema);

            MessageCreateParams parameters = new()
            {
                Model = Model,
                MaxTokens = 8000,
                // A region's prompt and schema are identical for every request, so the whole prefix is a cache read
                // after the first parse.
                System = new List<TextBlockParam>
                {
                    new()
                    {
                        Text = systemPrompt,
                        CacheControl = new CacheControlEphemeral(),
                    },
                },
                OutputConfig = new OutputConfig
                {
                    Effort = ParseEffort,
                    Format = new JsonOutputFormat { Schema = schema },
                },
                Messages = [new() { Role = Role.User, Content = transcript }],
            };

            // Sizes but not contents: the prompt and schema are both generated from source, so logging them every
            // parse is noise. The schema length is worth keeping because it is what the grammar limit reacts to.
            this.logger.LogInformation(
                $"[{region.Name}] requesting. System prompt {systemPrompt.Length} chars, schema {schemaJson.Length} chars, "
                + $"{region.Groups.Count} groups, {region.Grids.Count} grids, {region.Notes.Count} notes.");

            Stopwatch stopwatch = Stopwatch.StartNew();
            Message response;
            try
            {
                response = await this.client.Messages.Create(parameters, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                // The api puts the useful part in the response body, which is on the exception message. Log it here
                // rather than relying on it surviving the trip to the error banner.
                this.logger.LogError(ex, $"[{region.Name}] parse failed after {stopwatch.ElapsedMilliseconds}ms against {Model}.");
                throw;
            }
            finally
            {
                stopwatch.Stop();
            }

            if (response.StopReason == "refusal")
            {
                throw new InvalidOperationException($"The model declined to parse the {region.Name} region of this transcript.");
            }

            string json = response.Content
                .Select(x => x.Value)
                .OfType<TextBlock>()
                .Select(x => x.Text)
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"The model returned no content for the {region.Name} region.");

            // Output tokens are what this call's latency actually tracks, so both are logged together: tuning the
            // model or the effort without knowing which of the two moved is guesswork.
            this.logger.LogInformation(
                $"[{region.Name}] parsed in {stopwatch.ElapsedMilliseconds}ms. Stop reason {response.StopReason}. "
                + $"Input {response.Usage.InputTokens}, cache write {response.Usage.CacheCreationInputTokens}, "
                + $"cache read {response.Usage.CacheReadInputTokens}, output {response.Usage.OutputTokens}.");
            this.logger.LogInformation($"[{region.Name}] response json:{Environment.NewLine}{json}");

            return Interpret(json, region, includeIntake);
        }

        /// <summary>
        /// The schema constrains which values are legal. This prompt only carries the things a schema cannot say:
        /// what the transcript is, what an unmentioned question means, and what to do when the words do not fit.
        /// </summary>
        private static string BuildSystemPrompt(
            ChiroVocabulary vocabulary,
            ChiroVocabularyRegion region,
            bool includeIntake,
            Dictionary<string, Options.ChiroClinic> clinics)
        {
            StringBuilder prompt = new();

            _ = prompt.AppendLine($"You transcribe {vocabulary.Species} veterinary chiropractic dictations onto a structured exam form.");
            _ = prompt.AppendLine("The user message is one visit's dictation. Fill in the form for that visit and nothing else.");
            _ = prompt.AppendLine();

            _ = prompt.AppendLine("## Your part of the form");
            _ = prompt.AppendLine($"You are filling in the {region.Name} fields only. The dictation covers the whole visit, so most of it will");
            _ = prompt.AppendLine("be about areas you are not being asked about. Ignore those parts completely rather than trying to fit them");
            _ = prompt.AppendLine("into a field you do have. Other requests are handling them.");
            _ = prompt.AppendLine("This applies to the notes fields too. A notes field is for its own area, not a catch-all: only write there");
            _ = prompt.AppendLine("if the words are about that specific area. Anatomy that belongs to another part of the body is not yours,");
            _ = prompt.AppendLine("however well it would fit. Leave a notes field null rather than filling it with something out of scope.");
            _ = prompt.AppendLine();

            _ = prompt.AppendLine("## The input is speech to text");
            _ = prompt.AppendLine("It contains recognition errors. Prefer a veterinary chiropractic reading over a literal one:");
            _ = prompt.AppendLine("\"cairo\" is almost always \"chiro\"; \"post\" is \"posterior\"; \"t4\" is \"T4\"; \"on the front\" means the forelimb.");
            _ = prompt.AppendLine("Terms you will see: PI and AS (pelvis), SCP (spinous/mammillary process), coxofemoral, cranial, caudal, ventral, dorsal, medial, lateral.");
            _ = prompt.AppendLine("Punctuation is unreliable. A side stated once carries across the items that follow it in the same phrase,");
            _ = prompt.AppendLine("so \"left digits 2, 3 on the front\" is digits 2 and 3 of the LEFT forelimb.");
            _ = prompt.AppendLine();

            _ = prompt.AppendLine("## Report only what was said");
            _ = prompt.AppendLine("The dictation names findings, not everything examined. Leave every question the dictation does not mention");
            _ = prompt.AppendLine("empty, and omit unmentioned spinal levels from the lists entirely. Filling those in is handled after you,");
            _ = prompt.AppendLine("so guessing at them only introduces errors. Never select an arbitrary option to avoid an empty question.");
            _ = prompt.AppendLine();

            _ = prompt.AppendLine("## What \"mobilization\" means");
            _ = prompt.AppendLine("Mobilization records that a site was assessed and needed no adjustment. It is therefore never correct");
            _ = prompt.AppendLine("alongside an actual finding for the same level: if a level was adjusted, it was not left alone. It also never");
            _ = prompt.AppendLine("carries across a list. Select it only where the dictation names it for that specific level, and never as an");
            _ = prompt.AppendLine("inference. Levels the dictation says nothing about are filled in after you, so leave them out entirely.");
            _ = prompt.AppendLine();

            _ = prompt.AppendLine("## Things that did not happen");
            _ = prompt.AppendLine("If the dictation says something was resisted, deferred, not tolerated, or not performed, do NOT select it.");
            _ = prompt.AppendLine("Record it in the notes for that area instead. An intended-but-not-done adjustment is not a finding.");
            _ = prompt.AppendLine();

            if (vocabulary.Glossary.Count > 0)
            {
                _ = prompt.AppendLine($"## {vocabulary.Species} terminology");
                foreach (string entry in vocabulary.Glossary)
                {
                    _ = prompt.AppendLine($"- {entry}");
                }

                _ = prompt.AppendLine();
            }

            if (region.Hints.Count > 0)
            {
                _ = prompt.AppendLine("## Rules for these particular fields");
                foreach (string hint in region.Hints)
                {
                    _ = prompt.AppendLine($"- {hint}");
                }

                _ = prompt.AppendLine();
            }

            _ = prompt.AppendLine("## When the words do not fit");
            _ = prompt.AppendLine("Never round a phrase to the nearest available option. If the dictation describes something this part of the");
            _ = prompt.AppendLine("form has no option for, put the phrase verbatim in the notes field for that area and raise a flag.");
            _ = prompt.AppendLine();

            if (includeIntake && clinics.Count > 0)
            {
                _ = prompt.AppendLine("## Clinics");
                foreach (KeyValuePair<string, Options.ChiroClinic> clinic in clinics)
                {
                    _ = prompt.AppendLine($"- {clinic.Key}: {clinic.Value.Name}");
                }

                _ = prompt.AppendLine("Only set the clinic when the dictation ties one to this visit. A clinic named as the site of a future");
                _ = prompt.AppendLine("appointment is not this visit's clinic; leave it null and flag it.");
                _ = prompt.AppendLine();
            }

            _ = prompt.AppendLine("## Flags");
            _ = prompt.AppendLine("A flag is a question for the person checking the form, not an explanation of your work. Raise one only when");
            _ = prompt.AppendLine("a careful reviewer of the same words might reasonably choose differently from you.");
            _ = prompt.AppendLine();
            _ = prompt.AppendLine("Flag: you picked one of two options that both fit; a phrase had no matching option at all; you could not tell");
            _ = prompt.AppendLine("which side or which level something belonged to.");
            _ = prompt.AppendLine();
            _ = prompt.AppendLine("Do NOT flag: following the instructions above (an unmentioned question left empty, something not performed");
            _ = prompt.AppendLine("recorded in notes, a future date or clinic left off) — those are rules, not judgement calls. Do not flag a");
            _ = prompt.AppendLine("reading you are confident in, however garbled the words were. Do not flag choosing the only option that could");
            _ = prompt.AppendLine("possibly apply. Do not flag to show your reasoning.");
            _ = prompt.AppendLine();
            _ = prompt.AppendLine("Keep the reason to one short sentence. Only flag things that concern the fields you were given. More than two");
            _ = prompt.AppendLine("or three flags from one request means you are explaining rather than flagging.");

            return prompt.ToString();
        }

        /// <summary>
        /// Re-checks everything against the vocabulary rather than trusting the response. The schema should make
        /// most of this impossible, but a value that reaches the form has to be a value the form can render, and
        /// anything that does not survive is preserved as text instead of being dropped.
        /// </summary>
        private static RegionParse Interpret(string json, ChiroVocabularyRegion region, bool includeIntake)
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            Dictionary<string, List<string>> notes = region.Notes.ToDictionary(x => x.Name, x => new List<string>());

            void Salvage(string notesName, string text)
            {
                if (notes.TryGetValue(notesName, out List<string> lines))
                {
                    lines.Add(text);
                }
            }

            foreach (ChiroVocabularyNote note in region.Notes)
            {
                string value = ReadString(root, note.Name);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    notes[note.Name].Add(value.Trim());
                }
            }

            Dictionary<string, string[]> groups = [];
            foreach (ChiroVocabularyGroup group in region.Groups)
            {
                string[] selected = Keep(root, group.Name, group.Choices, group.Label, group.NotesName, Salvage);
                groups[group.Name] = Resolve(selected, group.Choices, group.StandingSites);
            }

            Dictionary<string, IReadOnlyDictionary<string, string[]>> grids = [];
            foreach (ChiroVocabularyGrid grid in region.Grids)
            {
                Dictionary<string, string[]> rows = [];
                if (root.TryGetProperty(grid.Name, out JsonElement gridElement) && gridElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement entry in gridElement.EnumerateArray())
                    {
                        string row = ReadString(entry, "Row");
                        string match = Array.Find(grid.Rows, x => string.Equals(x, row, StringComparison.OrdinalIgnoreCase));
                        if (match == null)
                        {
                            Salvage(grid.NotesName, $"[unmatched] {grid.Label}: {row}");
                            continue;
                        }

                        string[] selections = Keep(entry, "Selections", grid.Columns, $"{grid.Label} {match}", grid.NotesName, Salvage);

                        // The model can list a level twice; the form cannot.
                        rows[match] = rows.TryGetValue(match, out string[] existing)
                            ? [.. existing.Concat(selections).Distinct()]
                            : selections;
                    }
                }

                foreach (string row in grid.Rows)
                {
                    rows[row] = Resolve(rows.TryGetValue(row, out string[] selections) ? selections : [], grid.Columns, []);
                }

                grids[grid.Name] = rows;
            }

            List<ChiroParseFlag> flags = [];
            if (root.TryGetProperty(ChiroSchemaBuilder.FlagsKey, out JsonElement flagsElement) && flagsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement flag in flagsElement.EnumerateArray())
                {
                    flags.Add(new ChiroParseFlag(
                        ReadString(flag, "SourceText") ?? string.Empty,
                        ReadString(flag, "Section") ?? string.Empty,
                        ReadString(flag, "Reason") ?? string.Empty));
                }
            }

            return new RegionParse
            {
                PatientName = includeIntake ? ReadString(root, ChiroSchemaBuilder.PatientNameKey) : null,
                OwnerName = includeIntake ? ReadString(root, ChiroSchemaBuilder.OwnerNameKey) : null,
                DateOfService = includeIntake && DateOnly.TryParseExact(
                    ReadString(root, ChiroSchemaBuilder.DateOfServiceKey),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateOnly dateOfService)
                        ? dateOfService
                        : null,
                ClinicAbbreviation = includeIntake ? ReadString(root, ChiroSchemaBuilder.ClinicAbbreviationKey) : null,
                Groups = groups,
                Grids = grids,
                Notes = notes.ToDictionary(x => x.Key, x => x.Value.Count == 0 ? null : string.Join(" ", x.Value)),
                Flags = flags,
            };
        }

        private static ChiroParseResult Merge(RegionParse[] parses)
        {
            Dictionary<string, string[]> groups = [];
            Dictionary<string, IReadOnlyDictionary<string, string[]>> grids = [];
            Dictionary<string, string> notes = [];
            List<ChiroParseFlag> flags = [];

            foreach (RegionParse parse in parses)
            {
                foreach (KeyValuePair<string, string[]> group in parse.Groups)
                {
                    groups[group.Key] = group.Value;
                }

                foreach (KeyValuePair<string, IReadOnlyDictionary<string, string[]>> grid in parse.Grids)
                {
                    grids[grid.Key] = grid.Value;
                }

                foreach (KeyValuePair<string, string> note in parse.Notes)
                {
                    notes[note.Key] = note.Value;
                }

                flags.AddRange(parse.Flags);
            }

            return new ChiroParseResult
            {
                // Only the intake region answers these, so first non null wins without any conflict to resolve.
                PatientName = parses.Select(x => x.PatientName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                OwnerName = parses.Select(x => x.OwnerName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                DateOfService = parses.Select(x => x.DateOfService).FirstOrDefault(x => x.HasValue),
                ClinicAbbreviation = parses.Select(x => x.ClinicAbbreviation).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                GroupSelections = groups,
                GridSelections = grids,
                Notes = notes,
                // Regions parse the same transcript, so the same phrase can be flagged by more than one of them.
                // Keyed on the quoted words alone: two regions describing the same ambiguity will word the reason
                // differently and name different fields, so anything narrower does not actually dedupe.
                Flags = [.. flags
                    .GroupBy(x => Normalize(x.SourceText))
                    .Select(x => x.First())],
            };
        }

        private const string Mobilization = "mobilization";

        private static bool IsMobilization(string choice)
            => choice.Contains(Mobilization, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// A mobilization option records that a site was assessed and needed no adjustment, so an unmentioned
        /// question means every mobilization option it offers. A question covering several joints, such as a rear
        /// limb, therefore comes back with one per joint.
        /// <para>
        /// Enforced here rather than asked of the model: these are rules, not judgements, and the model has been
        /// observed inferring mobilization across a run-on list of levels that were in fact adjusted.
        /// </para>
        /// </summary>
        private static string[] Resolve(string[] selected, string[] choices, string[] standingSites)
        {
            string[] mobilizations = [.. choices.Where(IsMobilization)];

            if (selected.Length == 0)
            {
                return mobilizations;
            }

            // Where the bare word is the only mobilization option it marks the whole question as needing nothing,
            // so it cannot stand alongside a finding for that question. Where the options name specific joints they
            // are per joint statements, and an adjustment to one joint says nothing about the others.
            bool isWholeQuestionMarker = mobilizations.Length == 1
                && string.Equals(mobilizations[0], Mobilization, StringComparison.OrdinalIgnoreCase);
            if (isWholeQuestionMarker)
            {
                string[] adjustments = [.. selected.Where(x => !string.Equals(x, Mobilization, StringComparison.OrdinalIgnoreCase))];
                return adjustments.Length == 0 ? selected : adjustments;
            }

            // A standing site is assessed every visit, so its mobilizations belong on the record unless the
            // dictation addressed that site. Findings elsewhere in the same question do not speak for it.
            List<string> resolved = [.. selected];
            foreach (string site in standingSites)
            {
                if (resolved.Exists(x => x.Contains(site, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                resolved.AddRange(choices.Where(x => IsMobilization(x) && x.Contains(site, StringComparison.OrdinalIgnoreCase)));
            }

            return [.. resolved.Distinct().OrderBy(x => Array.IndexOf(choices, x))];
        }

        private static string[] Keep(
            JsonElement parent,
            string property,
            string[] allowed,
            string label,
            string notesName,
            Action<string, string> salvage)
        {
            if (!parent.TryGetProperty(property, out JsonElement element) || element.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            List<string> kept = [];
            foreach (JsonElement item in element.EnumerateArray())
            {
                string value = item.GetString();
                string match = Array.Find(allowed, x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase));
                if (match == null)
                {
                    salvage(notesName, $"[unmatched] {label}: {value}");
                }
                else if (!kept.Contains(match))
                {
                    kept.Add(match);
                }
            }

            return [.. kept];
        }

        private static string Normalize(string text)
            => string.Join(' ', (text ?? string.Empty).ToLowerInvariant().Split((char[])null, StringSplitOptions.RemoveEmptyEntries)).Trim('.', ',', ';');

        private static string ReadString(JsonElement parent, string property)
            => parent.TryGetProperty(property, out JsonElement element) && element.ValueKind == JsonValueKind.String
                ? element.GetString()
                : null;

        /// <summary>One region's answers, before they are merged back into a single draft.</summary>
        private sealed class RegionParse
        {
            public string PatientName { get; init; }
            public string OwnerName { get; init; }
            public DateOnly? DateOfService { get; init; }
            public string ClinicAbbreviation { get; init; }
            public required Dictionary<string, string[]> Groups { get; init; }
            public required Dictionary<string, IReadOnlyDictionary<string, string[]>> Grids { get; init; }
            public required Dictionary<string, string> Notes { get; init; }
            public required List<ChiroParseFlag> Flags { get; init; }
        }
    }

    /// <summary>
    /// A draft of a form, plus the list of things the parser was not sure about. Neither is persisted; both exist
    /// only long enough for a human to look at the prefilled page.
    /// </summary>
    public sealed class ChiroParseResult
    {
        public string PatientName { get; init; }
        public string OwnerName { get; init; }
        public DateOnly? DateOfService { get; init; }
        public string ClinicAbbreviation { get; init; }

        /// <summary>Bound property name to the selected choices.</summary>
        public required IReadOnlyDictionary<string, string[]> GroupSelections { get; init; }

        /// <summary>Bound property name to row to the selected columns. The page flattens these to the posted form values.</summary>
        public required IReadOnlyDictionary<string, IReadOnlyDictionary<string, string[]>> GridSelections { get; init; }

        /// <summary>Bound property name to note text, or null when the parser had nothing to say.</summary>
        public required IReadOnlyDictionary<string, string> Notes { get; init; }

        public required IReadOnlyList<ChiroParseFlag> Flags { get; init; }
    }

    /// <param name="SourceText">The words from the dictation that caused the flag.</param>
    /// <param name="Section">The bound property name the flag concerns, if any.</param>
    /// <param name="Reason">What is uncertain, and what the parser did about it.</param>
    public sealed record ChiroParseFlag(string SourceText, string Section, string Reason);
}
