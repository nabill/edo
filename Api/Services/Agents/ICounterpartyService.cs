using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface ICounterpartyService
    {
        Task<Result<SlimCounterpartyInfo>> Add(CounterpartyCreateRequest counterparty);

        Task<Result<SlimCounterpartyInfo>> Get(int counterpartyId);

        Task<Result<CounterpartyContractKind>> GetContractKind(int counterpartyId);

        //Task<Result<Agency>> AddAgency(int counterpartyId, AgencyInfo agency);

        Task<Agency> GetRootAgency(int counterpartyId);
    }
}