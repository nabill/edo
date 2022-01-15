using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Data.AccommodationMappings
{
    public class AccommodationDuplicateReport
    {
        public int Id { get; set; }
        public int ReporterAgentId { get; set; }
        public int ReporterAgencyId { get; set; }
        public AccommodationDuplicateReportState ApprovalState { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Modified { get; set; }
        public List<SupplierAccommodationId> Accommodations { get; set; }
        public int? EditorAdministratorId { get; set; }
    }
}