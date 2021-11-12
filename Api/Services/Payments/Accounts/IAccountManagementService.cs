using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public interface IAccountManagementService
    {
        Task<Result> CreateForAgency(Agency agency, Currencies currency);

        Task<Result<AgencyAccount>> Get(int agencyId, Currencies currency);
    }
}