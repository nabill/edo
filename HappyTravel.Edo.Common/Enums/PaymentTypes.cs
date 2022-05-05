using System.ComponentModel;

namespace HappyTravel.Edo.Common.Enums
{
    public enum PaymentTypes
    {
        [Description("Other")] 
        NotSpecified = 0,
        
        [Description("Offline")] 
        Offline = 1,
        
        [Description("Credit card")] 
        CreditCard = 2,
        
        [Description("Virtual account")] 
        VirtualAccount = 3,
    }
}