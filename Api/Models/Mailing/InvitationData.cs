namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class InvitationData : DataWithCompanyInfo
    {
        public string AgencyName { get; set; }
        public string InvitationCode { get; set; }
        public string UserEmailAddress { get; set; }
        public string UserName { get; set; }
    }
}