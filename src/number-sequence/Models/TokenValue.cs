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

        public static TokenValue CreateFrom(Token token) =>
            new()
            {
                Account = token.Account,
                AccountTier = token.AccountTier,
                Name = token.Name,
                CreatedDate = token.CreatedDate,
                ExpirationDate = token.ExpirationDate,
                Key = StringUtilities.GetRandomAlphanumericString(128)
            };

        [JsonPropertyName("a"), Required, MinLength(3), MaxLength(64)]
        public string Account { get; set; }

        [JsonPropertyName("t"), Required]
        public AccountTier AccountTier { get; set; }

        [JsonPropertyName("n"), Required, MinLength(3), MaxLength(64)]
        public string Name { get; set; }

        [JsonPropertyName("c"), Required]
        public DateTimeOffset CreatedDate { get; set; }

        [JsonPropertyName("e"), Required, CustomValidation(typeof(TokenValidation), nameof(TokenValidation.ExpirationValidation))]
        public DateTimeOffset ExpirationDate { get; set; }

        [JsonPropertyName("k"), Required, MinLength(128), MaxLength(128)]
        public string Key { get; set; }
    }
}
