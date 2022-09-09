using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public class AccountManagementTransitionalService : IAccountManagementTransitionalService
    {
        public AccountManagementTransitionalService(IAccountManagementService accountManagementService)
        {
            _accountManagementService = accountManagementService;
        }


        public Task<Result> CreateForAgency(Agency agency, Currencies currency)
            => _accountManagementService.CreateForAgency(agency, currency);


        public Task<Result<AgencyAccount>> Get(int agencyId, Currencies currency)
            => _accountManagementService.Get(agencyId, currency);


        private readonly IAccountManagementService _accountManagementService;
    }
}