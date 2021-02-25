using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface ICounterpartyService
    {
        Task<Result<CounterpartyInfo>> Add(CounterpartyEditRequest counterparty);

        Task<Result<CounterpartyInfo>> Get(int counterpartyId, string languageCode = LocalizationHelper.DefaultLanguageCode);

        //Task<Result<Agency>> AddAgency(int counterpartyId, AgencyInfo agency);

        Task<Agency> GetDefaultAgency(int counterpartyId);
    }
}