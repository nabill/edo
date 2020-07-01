using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class RegistrationDataForAdmin : DataWithCompanyInfo
    {
        public CounterpartyEditRequest Counterparty { get; set; }
        public string AgentEmail { get; set; }
        public string AgentName { get; set; }
    }
}