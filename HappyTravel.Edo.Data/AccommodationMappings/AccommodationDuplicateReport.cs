using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.AccommodationMappings
{
    public class AccommodationDuplicateReport
    {
        public int Id { get; set; }
        public ProviderAccommodationId Accommodation { get; set; }
        public List<ProviderAccommodationId> Duplicates { get; set; }
        public int ReporterAgentId { get; set; }
        public int ReporterAgencyId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }

    public class ProviderAccommodationId
    {
        public string AccommodationId { get; set; }
        public DataProviders DataProvider { get; set; }
    }
}