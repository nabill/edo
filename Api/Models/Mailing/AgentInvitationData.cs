namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class AgentInvitationData : DataWithCompanyInfo
    {
        public string CounterpartyName { get; set; }
        public string InvitationCode { get; set; }
        public string UserEmailAddress { get; set; }
        public string UserName { get; set; }
    }
}