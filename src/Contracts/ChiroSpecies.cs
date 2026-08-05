using System.Text.Json.Serialization;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// The species a chiro record was captured for. Drives the diagram, the email subject, and the expected lengths of the species-specific arrays on <see cref="ChiroInput"/>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChiroSpecies
    {
        /// <summary>Not specified. Never valid on a submitted record.</summary>
        Unknown,

        Canine,
        Equine,
    }
}
