using HappyTravel.Edo.Api.Models.Mailing;

namespace Api.Models.Mailing
{
    public class CreditLimitData : DataWithCompanyInfo
    {
        public int Percentage { get; set; }
        public string AgencyName { get; set; } = null!;
        public string? ContactDetails { get; set; }
    }
}