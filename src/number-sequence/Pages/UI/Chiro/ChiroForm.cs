using number_sequence.Services;
using number_sequence.Utilities;
using System.Reflection;

namespace number_sequence.Pages.UI.Chiro
{
    /// <summary>
    /// Shared rendering and mapping helpers for the species-specific chiro forms.
    /// A species owns its own fields and choices unless its form is identical to another's, which is why canine and
    /// feline share <see cref="SmallAnimalChiroFormModel"/> and equine does not.
    /// </summary>
    public static class ChiroForm
    {
        /// <summary>
        /// Grid checkboxes post a single flat array of "row|column" values so that each grid needs only one
        /// bound property instead of one per row.
        /// </summary>
        public const char GridSeparator = '|';

        /// <summary>
        /// Multi-select answers are recorded as a single comma-separated string, matching how google sheets
        /// serializes them, so that pdfs generated from either ingestion path are identical.
        /// </summary>
        public static string Join(string[] selected) => string.Join(", ", selected ?? []);

        /// <summary>
        /// Flatten the posted "row|column" values into one joined string per row, in the declared row and
        /// column order rather than the order the browser happened to post them.
        /// </summary>
        public static string[] JoinGrid(string[] selected, string[] rows, string[] columns)
        {
            Dictionary<string, List<string>> selectedByRow = rows.ToDictionary(x => x, _ => new List<string>());

            foreach (string value in selected ?? [])
            {
                string[] parts = value.Split(GridSeparator, 2);
                if (parts.Length == 2 && selectedByRow.TryGetValue(parts[0], out List<string> selectedColumns))
                {
                    selectedColumns.Add(parts[1]);
                }
            }

            return rows
                .Select(row => string.Join(", ", selectedByRow[row].OrderBy(column => Array.IndexOf(columns, column))))
                .ToArray();
        }

        /// <summary>
        /// Writes a parsed draft onto a species form.
        /// <para>
        /// Reflection rather than a per-species switch: every name in a <see cref="ChiroVocabulary"/> comes from a
        /// <c>nameof</c> on the bound property it describes, so the lookup cannot go stale, and a new species needs
        /// no changes here.
        /// </para>
        /// </summary>
        public static void ApplyParse(object model, ChiroVocabulary vocabulary, ChiroParseResult result)
        {
            Type type = model.GetType();

            void Set(string name, object value)
            {
                PropertyInfo property = type.GetProperty(name)
                    ?? throw new InvalidOperationException($"{type.Name} has no [{name}] property, but its vocabulary names one.");
                property.SetValue(model, value);
            }

            foreach (ChiroVocabularyGroup group in vocabulary.Groups)
            {
                if (result.GroupSelections.TryGetValue(group.Name, out string[] selected))
                {
                    Set(group.Name, selected);
                }
            }

            foreach (ChiroVocabularyGrid grid in vocabulary.Grids)
            {
                if (!result.GridSelections.TryGetValue(grid.Name, out IReadOnlyDictionary<string, string[]> rows))
                {
                    continue;
                }

                // Back to the flat "row|column" shape the checkboxes post, in declared order.
                List<string> flattened = [];
                foreach (string row in grid.Rows)
                {
                    if (rows.TryGetValue(row, out string[] columns))
                    {
                        flattened.AddRange(columns.Select(x => row + GridSeparator + x));
                    }
                }

                Set(grid.Name, flattened.ToArray());
            }

            foreach (ChiroVocabularyNote note in vocabulary.Notes)
            {
                if (result.Notes.TryGetValue(note.Name, out string text) && !string.IsNullOrWhiteSpace(text))
                {
                    Set(note.Name, text);
                }
            }
        }

        /// <summary>
        /// Additional recipients are entered as free text and batched up separately from the primary recipient.
        /// </summary>
        public static string[] SplitEmails(string emails)
            => (emails ?? string.Empty).Split([',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// The sticky header shared across every species form: back link, species title, and the in-page section
    /// jump links. All species currently share the same sections, so <see cref="Sections"/> defaults to them and
    /// a divergent species can override the list.
    /// </summary>
    public sealed class ChiroFormHeader
    {
        public string Species { get; init; }
        public string BackUrl { get; init; } = "/ui/chiro";
        public IReadOnlyList<(string Anchor, string Label)> Sections { get; init; } = DefaultSections;

        public static readonly IReadOnlyList<(string Anchor, string Label)> DefaultSections = new[]
        {
            ("head", "Head"),
            ("cervical", "Cervical"),
            ("thoracic", "Thoracic"),
            ("lumbar", "Lumbar"),
            ("sacrum-pelvis", "Sacrum / Pelvis"),
            ("left-forelimb", "Left Forelimb"),
            ("right-forelimb", "Right Forelimb"),
            ("left-rear-limb", "Left Rear Limb"),
            ("right-rear-limb", "Right Rear Limb"),
            ("coccygeal", "Coccygeal / Other"),
        };
    }

    /// <summary>
    /// A single multi-select question.
    /// </summary>
    public sealed class ChiroCheckboxGroup
    {
        public string Label { get; init; }
        public string Name { get; init; }
        public string[] Choices { get; init; }
        public string[] Selected { get; init; }
    }

    /// <summary>
    /// A multi-select question repeated across a set of rows.
    /// </summary>
    public sealed class ChiroCheckboxGrid
    {
        public string Label { get; init; }
        public string Name { get; init; }
        public string[] Rows { get; init; }
        public string[] Columns { get; init; }
        public string[] Selected { get; init; }
    }
}
