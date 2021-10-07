using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Data.Markup
{
    public class AgencyMarkupBonusesAccount : IEntity
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public Currencies Currency { get; set; }
        public decimal Balance { get; set; }
    }
}