using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface ICounterpartyManagementService
    {
        Task<Result<CounterpartyInfo>> Get(int counterpartyId, string languageCode);

        Task<List<CounterpartyInfo>> Get(string languageCode);

        Task<List<CounterpartyPrediction>> GetCounterpartiesPredictions(string query);

        Task<Result<List<AgencyInfo>>> GetAllCounterpartyAgencies(int counterpartyId);

        Task<Result<CounterpartyInfo>> Update(CounterpartyEditRequest counterparty, int counterpartyId, string languageCode);

        Task<Result> DeactivateCounterparty(int counterpartyId, string reason);

        Task<Result> ActivateCounterparty(int counterpartyId, string reason);
    }
}