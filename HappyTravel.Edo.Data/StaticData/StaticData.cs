using System.Text.Json;
using HappyTravel.Edo.Data.StaticDatas;

namespace HappyTravel.Edo.Data.StaticData
{
    public class StaticData
    {
        public StaticDataTypes Type { get; set; }
        public JsonDocument Data { get; set; }
    }
}