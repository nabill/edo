namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class RegistrationDataForMaster : DataWithCompanyInfo
    {
        public string AgentName { get; set; }
        public string Position { get; set; }
        public string Title { get; set; }
    }
}