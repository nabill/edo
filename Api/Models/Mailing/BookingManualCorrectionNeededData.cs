namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class BookingManualCorrectionNeededData : DataWithCompanyInfo
    {
        public string ReferenceCode { get; set; }
        public string AgentName { get; set; }
        public string AgencyName { get; set; }
        public string Deadline { get; set; }
    }
}