using HappyTravel.Edo.Common.Enums.AgencySettings;

namespace HappyTravel.Edo.Data.Agents
{
    public class AgencySystemSettings
    {
        public int AgencyId { get; set; }

        public AprSettings? AdvancedPurchaseRatesSettings { get; set; } = AprSettings.DisplayOnly;
        
        public AgencyAvailabilitySearchSettings AvailabilitySearchSettings { get; set; }

        public DisplayedPaymentOptionsSettings? DisplayedPaymentOptions { get; set; }
    }
}