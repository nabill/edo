using System.Text.Json;

namespace HappyTravel.Edo.Data.StaticData
{
    public class StaticData
    {
        public StaticDataTypes Type { get; set; }
        public JsonDocument Data { get; set; } = null!;
    }
}