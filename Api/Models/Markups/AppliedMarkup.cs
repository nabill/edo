using HappyTravel.Edo.Data.Markup;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public class AppliedMarkup
    {
        public int PolicyId { get; set; }
        public MarkupPolicyScope Scope { get; set; }
        public MoneyAmount AmountChange { get; set; }
    }
}