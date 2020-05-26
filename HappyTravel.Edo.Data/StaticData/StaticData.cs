using System.Text.Json;

namespace HappyTravel.Edo.Data.StaticDatas
{
    public class StaticData
    {
        public StaticDataTypes Type { get; set; }
        public JsonDocument Data { get; set; }
    }
}