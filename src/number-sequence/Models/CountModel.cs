using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using number_sequence.Converters;
using System;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Interfaces;

namespace number_sequence.Models
{
    public sealed class CountModel : ICount
    {
        public string Account { get; set; }
        [JsonProperty("id")]
        public string Name { get; set; }
        [JsonConverter(typeof(UlongCosmosConverter))]
        public ulong Value { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        [JsonProperty("_ts"), JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTimeOffset ModifiedAt { get; set; }

        public string PK => $"{nameof(Count)}|{this.Account}".ToLower();

        public static implicit operator Count(CountModel c) => new Count
        {
            Account = c.Account,
            Name = c.Name,
            Value = c.Value,
            CreatedAt = c.CreatedAt,
            ModifiedAt = c.ModifiedAt
        };
        public static implicit operator CountModel(Count c) => new CountModel
        {
            Account = c.Account,
            Name = c.Name,
            Value = c.Value,
            CreatedAt = c.CreatedAt,
            ModifiedAt = c.ModifiedAt
        };
    }
}
