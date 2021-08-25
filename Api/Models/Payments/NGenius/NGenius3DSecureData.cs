using System.Text.Json.Serialization;

namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct NGenius3DSecureData
    {
        [JsonPropertyName("PaRes")]
        public string PaRes { get; init; }
    }
}