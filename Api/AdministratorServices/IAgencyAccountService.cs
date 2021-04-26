using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgencyAccountService
    {
        Task<List<FullAgencyAccountInfo>> GetAgencyAccounts(int agencyId);

        Task<Result> ActivateAgencyAccount(int agencyId, int agencyAccountId, string reason);

        Task<Result> DeactivateAgencyAccount(int agencyId, int agencyAccountId, string reason);

        Task<Result> IncreaseManually(int agencyAccountId, PaymentData paymentData, ApiCaller apiCaller);

        Task<Result> DecreaseManually(int agencyAccountId, PaymentData paymentData, ApiCaller apiCaller);
    }
}