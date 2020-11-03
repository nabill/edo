using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.AgencySettings;

namespace HappyTravel.Edo.Data.Agents
{
    public class AgencyAccommodationBookingSettings
    {
        public bool IsMarkupDisabled { get; set; }
        
        public AprMode? AprMode { get; set; }
        
        public PassedDeadlineOffersMode? PassedDeadlineOffersMode { get; set; }
        
        public List<Common.Enums.Suppliers> EnabledProviders { get; set; }
        
        public bool IsDataProviderVisible { get; set; }
    }
}