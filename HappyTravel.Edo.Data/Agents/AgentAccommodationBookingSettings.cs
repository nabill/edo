using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Data.Agents
{
    public class AgentAccommodationBookingSettings
    {
        /// <summary>
        /// Enabled suppliers list
        /// </summary>
        public List<Common.Enums.Suppliers> EnabledSuppliers { get; set; }
        
        public AprMode? AprMode { get; set; }
        
        public PassedDeadlineOffersMode? PassedDeadlineOffersMode { get; set; }
        
        public bool IsMarkupDisabled { get; set; }
        
        public bool IsSupplierVisible { get; set; }
        
        public bool AreTagsVisible { get; set; }
        
        public SearchFilters AdditionalSearchFilters { get; set; }
    }
}