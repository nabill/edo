namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class AdministratorInvitationData : DataWithCompanyInfo
    {
        public string InvitationCode { get; set; }
        public string UserEmailAddress { get; set; }
        public string UserName { get; set; }
    }
}