using System.Text.Json;

namespace HappyTravel.Edo.Data.Locations
{
    public class Market
    {
        public int Id { get; set; }
        public JsonDocument Names { get; set; } = null!;
    }
}
