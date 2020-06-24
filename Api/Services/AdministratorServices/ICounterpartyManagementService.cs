using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.AdministratorServices
{
    public interface ICounterpartyManagementService
    {
        Task<Result<CounterpartyInfo>> Get(int counterpartyId);

        Task<Result<List<CounterpartyInfo>>> Get();

        Task<Result<List<AgencyInfo>>> GetAllCounterpartyAgencies(int counterpartyId);

        Task<Result<CounterpartyInfo>> Update(CounterpartyInfo counterparty, int counterpartyId);

        Task<Result> VerifyAsFullyAccessed(int counterpartyId, string verificationReason);

        Task<Result> VerifyAsReadOnly(int counterpartyId, string verificationReason);
    }
}