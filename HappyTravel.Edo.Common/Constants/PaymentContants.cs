using HappyTravel.Edo.Common.Enums;
using System.Collections.Generic;

namespace HappyTravel.Edo.Common.Constants
{
    public static class PaymentContants
    {
        public static readonly Dictionary<Currencies, int> Multipliers = new Dictionary<Currencies, int>()
        {
            { Currencies.EUR, 100},
            { Currencies.USD, 100}
        };
    }
}
