using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.AgencySettings;

namespace HappyTravel.Edo.Data.Agents
{
    public class AgencyAvailabilitySearchSettings
    {
        public bool IsMarkupDisabled { get; set; }
        
        public AprMode? AprMode { get; set; }
        
        public PassedDeadlineOffersMode? PassedDeadlineOffersMode { get; set; }
        
        public List<DataProviders> EnabledProviders { get; set; }
    }
}