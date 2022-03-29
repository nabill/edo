using System.Text.Json;

namespace HappyTravel.Edo.Data.Locations
{
    public class Country
    {
        public string Code { get; set; } = string.Empty;
        public JsonDocument Names { get; set; } = null!;
        public int MarketId { get; set; }
    }
}
