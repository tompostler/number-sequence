using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace number_sequence.Utilities
{
    /// <summary>
    /// The serializer options for every json this app reads or writes outside of mvc (which is configured the same way in
    /// <c>Startup</c>). The default encoder escapes all non-ascii text as <c>\uXXXX</c>, which makes stored json unreadable
    /// in the db for accented names and curly quotes. Escaping for html is done at display time by razor instead, so a
    /// stored value must never be emitted with <c>Html.Raw</c>.
    /// <para>
    /// Enums are written by name, so reordering an enum can't change what a stored value means. Reads must use these
    /// options too: default options can't parse a named enum. The converter still reads numbers, which is how json stored
    /// before this (e.g. older <c>ChiroRecord.InputJson</c> rows) keeps working.
    /// </para>
    /// </summary>
    public static class RelaxedJson
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() },
        };
    }
}
