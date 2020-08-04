using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Data.AccommodationMappings
{
    public class AccommodationDuplicateReport
    {
        public int Id { get; set; }
        public int ReporterAgentId { get; set; }
        public int ReporterAgencyId { get; set; }
        public bool IsApproved { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}