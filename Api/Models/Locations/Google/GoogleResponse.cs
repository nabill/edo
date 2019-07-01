using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Locations.Google
{
    public class GoogleResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
