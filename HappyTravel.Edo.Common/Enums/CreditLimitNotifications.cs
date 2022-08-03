using System.ComponentModel;

namespace HappyTravel.Edo.Common.Enums
{
    public enum CreditLimitNotifications
    {
        [Description("Balance is more than 40%")]
        MoreThanFourty = 0,

        [Description("Balance is 40% or less")]
        FourtyOrLess = 40,

        [Description("Balance is 20% or less")]
        TwentyOrLess = 20,

        [Description("Balance is 10% or less")]
        TenOrLess = 10
    }
}