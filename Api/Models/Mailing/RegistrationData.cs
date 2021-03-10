namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class RegistrationData : DataWithCompanyInfo
    {
        public string AgentName { get; set; }
        public string Position { get; set; }
        public string Title { get; set; }
        public string AgencyName { get; set; }
    }
}