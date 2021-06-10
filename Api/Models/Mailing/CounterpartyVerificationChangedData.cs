using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class CounterpartyVerificationChangedData : DataWithCompanyInfo
    {
        public string AgentName { get; set; }
        public string CounterpartyName { get; set; }
        public CounterpartyStates State { get; set; }
        public string VerificationReason { get; set; }
    }
}