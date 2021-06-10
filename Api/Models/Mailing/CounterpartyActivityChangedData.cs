using HappyTravel.Edo.Api.Models.Management.Enums;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class CounterpartyActivityChangedData : DataWithCompanyInfo
    {
        public string AgentName { get; set; }
        public string CounterpartyName { get; set; }
        public ActivityStatus Status { get; set; }
    }
}
