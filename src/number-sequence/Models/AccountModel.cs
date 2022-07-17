using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Interfaces;

namespace number_sequence.Models
{
    public sealed class AccountModel : IAccount
    {
        [JsonProperty("id")]
        public string Name { get; set; }
        public string Key { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public AccountTier Tier { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedFrom { get; set; }
        [JsonProperty("_ts"), JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTimeOffset ModifiedAt { get; set; }

        public string PK { get; set; } = nameof(Account);

        public static implicit operator Account(AccountModel a) => new()
        {
            Name = a.Name,
            Key = a.Key,
            Tier = a.Tier,
            CreatedAt = a.CreatedAt,
            CreatedFrom = a.CreatedFrom,
            ModifiedAt = a.ModifiedAt
        };
        public static implicit operator AccountModel(Account a) => new()
        {
            Name = a.Name,
            Key = a.Key,
            Tier = a.Tier,
            CreatedAt = a.CreatedAt,
            CreatedFrom = a.CreatedFrom,
            ModifiedAt = a.ModifiedAt
        };
    }
}
