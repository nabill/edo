using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    /// <summary>
    ///     Settings for payfort tokenization
    /// </summary>
    public readonly struct TokenizationSettings
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="accessCode">Access Code</param>
        /// <param name="identifier">Merchant Identifier</param>
        /// <param name="tokenizationUrl">Payfort tokenization url</param>
        [JsonConstructor]
        public TokenizationSettings(string accessCode, string identifier, string tokenizationUrl)
        {
            AccessCode = accessCode;
            Identifier = identifier;
            TokenizationUrl = tokenizationUrl;
        }

        /// <summary>
        ///     Access Code
        /// </summary>
        public string AccessCode { get; }
        /// <summary>
        ///     Merchant Identifier
        /// </summary>
        public string Identifier { get; }
        /// <summary>
        /// Payfort tokenization url
        /// </summary>
        public string TokenizationUrl { get; }
    }
}
