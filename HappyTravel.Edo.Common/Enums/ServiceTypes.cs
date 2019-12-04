using System.ComponentModel;

namespace HappyTravel.Edo.Common.Enums
{
    public enum ServiceTypes
    {
        [Description("Hotel Booking")] 
        HTL = 0,
        [Description("Transfer")] 
        TRN = 1,
        [Description("Custom service")]
        CMS = 2
    }
}