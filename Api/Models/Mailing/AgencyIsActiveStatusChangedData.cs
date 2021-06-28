namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class AgencyIsActiveStatusChangedData : DataWithCompanyInfo
    {
        public string AgentName { get; set; }
        public string AgencyName { get; set; }
        public string Status { get; set; }
    }
}
