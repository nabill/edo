using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.AgencySettings;

namespace HappyTravel.Edo.Data.Agents
{
    public class AgentAccommodationBookingSettings
    {
        /// <summary>
        /// Enabled providers list
        /// </summary>
        public List<DataProviders> EnabledProviders { get; set; }
        
        public AprMode? AprMode { get; set; }
        
        public PassedDeadlineOffersMode? PassedDeadlineOffersMode { get; set; }
        
        public bool IsMarkupDisabled { get; set; }
        
        public bool IsDataProviderVisible { get; set; }
    }
}