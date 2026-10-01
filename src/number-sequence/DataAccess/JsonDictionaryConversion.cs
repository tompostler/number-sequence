using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using number_sequence.Utilities;
using System.Text.Json;

namespace number_sequence.DataAccess
{
    /// <summary>
    /// Stores a string dictionary as a json column. Registered as a convention in <see cref="NsContext"/>, so any
    /// <c>Dictionary&lt;string, string&gt;</c> property on an entity gets it without per-property configuration.
    /// </summary>
    public sealed class JsonDictionaryConverter : ValueConverter<Dictionary<string, string>, string>
    {
        public JsonDictionaryConverter()
            : base(
                x => x == null ? null : JsonSerializer.Serialize(x, RelaxedJson.Options),
                x => x == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(x, RelaxedJson.Options))
        { }
    }

    /// <summary>
    /// Lets change tracking notice an edit to an entry of a <see cref="JsonDictionaryConverter"/> column, rather than only a
    /// replacement of the whole dictionary.
    /// </summary>
    public sealed class JsonDictionaryComparer : ValueComparer<Dictionary<string, string>>
    {
        public JsonDictionaryComparer()
            : base(
                (a, b) => a == null ? b == null : b != null && a.Count == b.Count && !a.Except(b).Any(),
                x => x == null ? 0 : x.Aggregate(0, (hash, kvp) => hash ^ HashCode.Combine(kvp.Key, kvp.Value)),
                x => x == null ? null : new Dictionary<string, string>(x))
        { }
    }
}
