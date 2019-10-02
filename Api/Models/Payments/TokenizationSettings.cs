using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct TokenizationSettings
    {
        [JsonConstructor]
        public TokenizationSettings(string accessCode, string identifier, string tokenizationUrl)
        {
            AccessCode = accessCode;
            Identifier = identifier;
            TokenizationUrl = tokenizationUrl;
        }

        public string AccessCode { get; }
        public string Identifier { get; }
        public string TokenizationUrl { get; }
    }
}
