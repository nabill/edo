using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Payments;

namespace HappyTravel.Edo.Api.AdministratorServices.Models
{
    public class AgencyBalanceLimitInfo
    {
        public AgencyBalanceLimitInfo(Agency agency, AgencyAccount account)
        {
            AgencyId = agency.Id;
            if (agency.CreditLimit!.Value.Currency == account.Currency)
            {
                Balance = account.Balance;
                CreditLimit = agency.CreditLimit!.Value.Amount;
            }
        }

        public int AgencyId { get; }
        public decimal Balance { get; } = 0m;
        public decimal CreditLimit { get; } = 0m;
    }
}