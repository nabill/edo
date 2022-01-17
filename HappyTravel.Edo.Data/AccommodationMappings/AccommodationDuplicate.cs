using System;

namespace HappyTravel.Edo.Data.AccommodationMappings
{
    public class AccommodationDuplicate
    {
        public int Id { get; set; }
        public int ParentReportId { get; set; }
        public string AccommodationId1 { get; set; }
        public string AccommodationId2 { get; set; }
        public int ReporterAgentId { get; set; }
        public int ReporterAgencyId { get; set; }
        public DateTimeOffset Created { get; set; }
        public bool IsApproved { get; set; }
    }
}