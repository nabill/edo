using System.Text.Json;

namespace HappyTravel.Edo.Data.Locations
{
    public class Country
    {
        public string Code { get; set; }
        public JsonDocument Names { get; set; }
        public int RegionId { get; set; }
    }
}
