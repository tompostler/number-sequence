using number_sequence.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.Models
{
    public sealed class TokenValue
    {
        public TokenValue() { }

        public static TokenValue CreateFrom(TokenModel tokenModel) =>
            new TokenValue
            {
                Account = tokenModel.Account,
                AccountTier = tokenModel.AccountTier,
                Name = tokenModel.Name,
                CreatedAt = tokenModel.CreatedAt,
                ExpiresAt = tokenModel.ExpiresAt,
                Key = StringUtilities.GetRandomAlphanumericString(128)
            };

        [JsonPropertyName("a"), Required, MinLength(3), MaxLength(64)]
        public string Account { get; set; }

        [JsonPropertyName("t"), Required]
        public AccountTier AccountTier { get; set; }

        [JsonPropertyName("n"), Required, MinLength(3), MaxLength(64)]
        public string Name { get; set; }

        [JsonPropertyName("c"), Required]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("e"), Required, CustomValidation(typeof(TokenValidation), nameof(TokenValidation.ExpirationValidation))]
        public DateTimeOffset ExpiresAt { get; set; }

        [JsonPropertyName("k"), Required, MinLength(128), MaxLength(128)]
        public string Key { get; set; }
    }
}
