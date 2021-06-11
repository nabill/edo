using HappyTravel.Edo.Api.Models.Management.Enums;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class AgencyActivityChangedData : DataWithCompanyInfo
    {
        public string AgentName { get; set; }
        public string AgencyName { get; set; }
        public ActivityStatus Status { get; set; }
    }
}
