using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using TcpWtf.NumberSequence.Contracts;
using TcpWtf.NumberSequence.Contracts.Interfaces;

namespace number_sequence.Models
{
    public sealed class TokenModel : IToken
    {
        public string Account { get; set; }
        [JsonIgnore]
        public string Key { get; set; }
        [JsonProperty("id")]
        public string Name { get; set; }
        public string Value { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        [JsonProperty("_ts"), JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTimeOffset ModifiedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }

        public string PK => $"{nameof(Token)}|{this.Name}".ToLower();

        public static implicit operator Token(TokenModel t) => new Token
        {
            Account = t.Account,
            Key = t.Key,
            Name = t.Name,
            Value = t.Value,
            CreatedAt = t.CreatedAt,
            ExpiresAt = t.ExpiresAt
        };
        public static implicit operator TokenModel(Token t) => new TokenModel
        {
            Account = t.Account,
            Key = t.Key,
            Name = t.Name,
            Value = t.Value,
            CreatedAt = t.CreatedAt,
            ExpiresAt = t.ExpiresAt
        };
    }
}
