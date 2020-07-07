using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.AdministratorServices
{
    public interface ICounterpartyManagementService
    {
        Task<Result<CounterpartyInfo>> Get(int counterpartyId, string languageCode);

        Task<List<CounterpartyInfo>> Get(string languageCode);

        Task<Result<List<AgencyInfo>>> GetAllCounterpartyAgencies(int counterpartyId);

        Task<Result<CounterpartyInfo>> Update(CounterpartyEditRequest counterparty, int counterpartyId, string languageCode);

        Task<Result> VerifyAsFullyAccessed(int counterpartyId, string verificationReason);

        Task<Result> VerifyAsReadOnly(int counterpartyId, string verificationReason);
    }
}