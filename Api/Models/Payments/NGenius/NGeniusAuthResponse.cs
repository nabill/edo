using System.Text.Json.Serialization;

namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public class NGeniusAuthResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }
    }
}