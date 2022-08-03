using HappyTravel.Edo.Api.Models.Mailing;

namespace Api.Models.Mailing
{
    public class CreditLimitData : DataWithCompanyInfo
    {
        public int percentage { get; set; }
        public string agentName { get; set; } = null!;
        public string contactDetails { get; set; } = null!;
    }
}