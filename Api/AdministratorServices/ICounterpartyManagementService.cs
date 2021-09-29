using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface ICounterpartyManagementService
    {
        Task<Result<CounterpartyInfo>> Get(int counterpartyId, string languageCode = LocalizationHelper.DefaultLanguageCode);

        Task<List<SlimCounterpartyInfo>> Get();

        Task<List<CounterpartyPrediction>> GetCounterpartiesPredictions(string query);

        Task<List<AgencyInfo>> GetAllCounterpartyAgencies(int counterpartyId, string languageCode = LocalizationHelper.DefaultLanguageCode);

        Task<Result<MasterAgentContext>> GetRootAgencyMasterAgent(int counterpartyId);

        Task<Result<CounterpartyInfo>> Update(CounterpartyEditRequest counterparty, int counterpartyId);

        Task<Result> DeactivateCounterparty(int counterpartyId, string reason, MasterAgentContext masterAgentContext = default);

        Task<Result> ActivateCounterparty(int counterpartyId, string reason);
    }
}