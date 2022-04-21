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
        public List<string> EnabledSuppliers { get; set; }
        
        public AprMode? AprMode { get; set; }
        
        public PassedDeadlineOffersMode? PassedDeadlineOffersMode { get; set; }
        
        public bool IsSupplierVisible { get; set; }
        
        public bool IsDirectContractFlagVisible { get; set; }
        
        public SearchFilters AdditionalSearchFilters { get; set; }
        
        public int? CustomDeadlineShift { get; set; }
    }
}