using HappyTravel.Edo.Common.Enums.AgencySettings;

namespace HappyTravel.Edo.Data.Agents
{
    public class AgencySystemSettings
    {
        public int AgencyId { get; set; }

        public AprMode? AdvancedPurchaseRatesSettings { get; set; } = AprMode.DisplayOnly;
        
        public AgencyAvailabilitySearchSettings AvailabilitySearchSettings { get; set; }

        public DisplayedPaymentOptionsSettings? DisplayedPaymentOptions { get; set; }
    }
}