namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class AgencyVerificationStateChangedData : DataWithCompanyInfo
    {
        public string AgentName { get; set; }
        public string AgencyName { get; set; }
        public string State { get; set; }
    }
}