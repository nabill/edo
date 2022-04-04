using HappyTravel.MultiLanguage;

namespace HappyTravel.Edo.Data.Locations
{
    public class Market
    {
        public int Id { get; set; }
        public MultiLanguage<string> Names { get; set; } = null!;
    }
}
