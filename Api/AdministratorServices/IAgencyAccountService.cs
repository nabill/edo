using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgencyAccountService
    {
        Task<Result> IncreaseManually(int agencyAccountId, PaymentData paymentData, UserInfo userInfo);

        Task<Result> DecreaseManually(int agencyAccountId, PaymentData paymentData, UserInfo userInfo);
    }
}