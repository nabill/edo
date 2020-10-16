using HappyTravel.Edo.Common.Enums.AgencySettings;

namespace HappyTravel.Edo.Data.Agents
{
    public class AgencySystemSettings
    {
        public int AgencyId { get; set; }

        public AgencyAccommodationBookingSettings AccommodationBookingSettings { get; set; }

        public DisplayedPaymentOptionsSettings? DisplayedPaymentOptions { get; set; }
    }
}