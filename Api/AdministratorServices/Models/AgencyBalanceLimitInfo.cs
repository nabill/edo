using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Payments;

namespace HappyTravel.Edo.Api.AdministratorServices.Models
{
    public class AgencyBalanceLimitInfo
    {
        public AgencyBalanceLimitInfo(Agency agency, AgencyAccount account)
        {
            Agency = agency;
            CreditLimitNotifications = agency.CreditLimitNotifications;
            if (agency.CreditLimit!.Value.Currency == account.Currency)
            {
                Balance = account.Balance;
                CreditLimit = agency.CreditLimit!.Value.Amount;
            }
        }

        public Agency Agency { get; }
        public decimal? Balance { get; } = null;
        public decimal? CreditLimit { get; } = null;
        public CreditLimitNotifications CreditLimitNotifications { get; set; }
    }
}