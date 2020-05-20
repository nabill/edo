namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class BookingCancelledData : DataWithCompanyInfo
    {
        public string AgentName { get; set; }
        public string ReferenceCode { get; set; }
    }
}