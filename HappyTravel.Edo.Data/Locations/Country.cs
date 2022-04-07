using HappyTravel.MultiLanguage;

namespace HappyTravel.Edo.Data.Locations
{
    public class Country
    {
        public string Code { get; set; } = string.Empty;
        public MultiLanguage<string> Names { get; set; } = null!;
        public int MarketId { get; set; }
        public int RegionId { get; set; }
    }
}
