using System.Text.Json.Serialization;

namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// What the pdf status chart of chiro forms per day segments each day's bar by.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChiroChartGrouping
    {
        /// <summary>The clinic on the filled-out form.</summary>
        Clinic,

        /// <summary>The account that submitted the form.</summary>
        Account,
    }
}
