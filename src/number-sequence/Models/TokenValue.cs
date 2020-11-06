using number_sequence.Utilities;
using System;

namespace number_sequence.Models
{
    public sealed class TokenValue
    {
        public TokenValue() { }

        public static TokenValue CreateFrom(TokenModel tokenModel) =>
            new TokenValue
            {
                acc = tokenModel.Account,
                nam = tokenModel.Name,
                cre = tokenModel.CreatedAt,
                exp = tokenModel.ExpiresAt,
                key = StringUtilities.GetRandomAlphanumericString(128)
            };

#pragma warning disable IDE1006 // Naming Styles

        public string acc { get; set; }
        public string nam { get; set; }
        public DateTimeOffset cre { get; set; }
        public DateTimeOffset exp { get; set; }
        public string key { get; set; }

#pragma warning restore IDE1006 // Naming Styles
    }
}
