using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgencyAccountService
    {
        Task<List<FullAgencyAccountInfo>> Get(int agencyId);

        Task<List<FullAgencyAccountInfo>> Get(int agencyId, Currencies currency);

        Task<Result> Activate(int agencyId, int agencyAccountId, string reason);

        Task<Result> Deactivate(int agencyId, int agencyAccountId, string reason);

        Task<Result> IncreaseManually(int agencyAccountId, PaymentData paymentData, ApiCaller apiCaller);

        Task<Result> DecreaseManually(int agencyAccountId, PaymentData paymentData, ApiCaller apiCaller);

        Task<Result> AddMoney(int agencyAccountId, PaymentData paymentData, ApiCaller apiCaller);
        
        Task<Result> Subtract(int agencyAccountId, PaymentData paymentData, ApiCaller apiCaller);
    }
}