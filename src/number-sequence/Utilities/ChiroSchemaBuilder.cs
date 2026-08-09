using System.Text.Json;
using System.Text.Json.Nodes;

namespace number_sequence.Utilities
{
    /// <summary>
    /// Turns a <see cref="ChiroVocabulary"/> into the json schema the model is constrained to answer with.
    /// <para>
    /// This is the load bearing guardrail. Every selectable value is an enum member generated from the same array
    /// the checkbox is rendered from, so the model cannot return an option that does not exist on the form. Prose
    /// in the prompt handles the judgement calls; it is not what keeps the values legal.
    /// </para>
    /// </summary>
    public static class ChiroSchemaBuilder
    {
        public const string PatientNameKey = "PatientName";
        public const string OwnerNameKey = "OwnerName";
        public const string DateOfServiceKey = "DateOfService";
        public const string ClinicAbbreviationKey = "ClinicAbbreviation";
        public const string FlagsKey = "Flags";

        /// <summary>
        /// Builds the schema for one region. The whole form at once compiles a grammar the api rejects, so the
        /// intake fields ride on a single region rather than being repeated: two requests answering the same
        /// question would only give two chances to disagree.
        /// </summary>
        public static Dictionary<string, JsonElement> Build(
            ChiroVocabularyRegion region,
            IReadOnlyCollection<string> clinicAbbreviations,
            bool includeIntake)
        {
            JsonObject properties = [];

            if (includeIntake)
            {
                properties[PatientNameKey] = NullableString("The patient (animal) name. Null if not stated.");
                properties[OwnerNameKey] = NullableString("The owner's name. Null if not stated. Do not infer an owner from a breed, a clinic, or an event name.");
                properties[DateOfServiceKey] = NullableString("Date of service as yyyy-MM-dd. Null unless the dictation states the date of this visit. A future appointment date is not the date of service.");
                properties[ClinicAbbreviationKey] = ClinicSchema(clinicAbbreviations);
            }

            foreach (ChiroVocabularyGroup group in region.Groups)
            {
                properties[group.Name] = new JsonObject
                {
                    ["type"] = "array",
                    ["description"] = $"{group.Label}. Empty unless the dictation mentions it.",
                    ["items"] = new JsonObject { ["enum"] = ToJsonArray(group.Choices) },
                };
            }

            foreach (ChiroVocabularyGrid grid in region.Grids)
            {
                // A row per property would give the grammar compiler one enum per row, which across all the grids
                // is enough to blow its size limit. As a list of mentioned rows it is two enums per grid instead of
                // one per row, and unmentioned rows cost no output tokens at all.
                properties[grid.Name] = new JsonObject
                {
                    ["type"] = "array",
                    ["description"] = $"{grid.Label}. Include an entry only for a level the dictation actually mentions.",
                    ["items"] = new JsonObject
                    {
                        ["type"] = "object",
                        ["additionalProperties"] = false,
                        ["required"] = new JsonArray("Row", "Selections"),
                        ["properties"] = new JsonObject
                        {
                            ["Row"] = new JsonObject { ["enum"] = ToJsonArray(grid.Rows) },
                            ["Selections"] = new JsonObject
                            {
                                ["type"] = "array",
                                ["items"] = new JsonObject { ["enum"] = ToJsonArray(grid.Columns) },
                            },
                        },
                    },
                };
            }

            foreach (ChiroVocabularyNote note in region.Notes)
            {
                properties[note.Name] = NullableString($"{note.Label}. Free text. Put anything the dictation says about this area that has no matching option here, in the dictation's own words.");
            }

            properties[FlagsKey] = new JsonObject
            {
                ["type"] = "array",
                ["description"] = "Everything a human needs to check. One entry per guess, unmappable phrase, or ambiguity.",
                ["items"] = new JsonObject
                {
                    ["type"] = "object",
                    ["additionalProperties"] = false,
                    ["required"] = new JsonArray("SourceText", "Section", "Reason"),
                    ["properties"] = new JsonObject
                    {
                        ["SourceText"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The words from the dictation that caused this, quoted exactly.",
                        },
                        ["Section"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The field name this concerns, matching a key in this schema. Use an empty string if it concerns no single field.",
                        },
                        ["Reason"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "One sentence on what is uncertain and what was done about it.",
                        },
                    },
                },
            };

            JsonObject schema = new()
            {
                ["type"] = "object",
                ["additionalProperties"] = false,
                ["required"] = ToJsonArray([.. properties.Select(x => x.Key)]),
                ["properties"] = properties,
            };

            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(schema.ToJsonString());
        }

        /// <summary>
        /// Structured outputs require every property to be listed as required, so "not set" has to be expressible
        /// as a value rather than as an absent key.
        /// </summary>
        private static JsonObject NullableString(string description)
            => new()
            {
                ["type"] = new JsonArray("string", "null"),
                ["description"] = description,
            };

        private static JsonObject ClinicSchema(IReadOnlyCollection<string> clinicAbbreviations)
        {
            const string Description = "The additional clinic to copy on this record, by abbreviation. Null unless the dictation ties a clinic to this visit; a clinic named only as the location of a future appointment is not this.";

            if (clinicAbbreviations.Count == 0)
            {
                return new JsonObject { ["type"] = "null", ["description"] = Description };
            }

            JsonArray options = ToJsonArray([.. clinicAbbreviations]);
            options.Add(null);
            return new JsonObject { ["enum"] = options, ["description"] = Description };
        }

        private static JsonArray ToJsonArray(string[] values)
        {
            JsonArray array = [];
            foreach (string value in values)
            {
                array.Add(value);
            }

            return array;
        }
    }
}
