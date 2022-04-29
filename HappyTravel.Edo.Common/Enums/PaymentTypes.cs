using System.ComponentModel;

namespace HappyTravel.Edo.Common.Enums
{
    public enum PaymentTypes
    {
        [Description("Other")] 
        NotSpecified = 0,
        
        [Description("Offline")] 
        Offline = 1,
        
        [Description("Credit Card")] 
        CreditCard = 2,
        
        [Description("VirtualAccount")] 
        VirtualAccount = 3,
    }
}