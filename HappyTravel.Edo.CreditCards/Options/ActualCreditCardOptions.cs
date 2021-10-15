using System.Collections.Generic;
using HappyTravel.Edo.CreditCards.Models;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.CreditCards.Options
{
    public class ActualCreditCardOptions
    {
        public Dictionary<Currencies, CreditCardInfo> Cards { get; set; } = new();
    }
}