using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface ICounterpartyService
    {
        Task<Result<Counterparty>> Add(CounterpartyInfo counterparty);

        Task<Result<CounterpartyInfo>> Get(int counterpartyId);

        Task<Result<CounterpartyInfo>> Update(CounterpartyInfo counterparty, int counterpartyId);

        Task<Result<Agency>> AddAgency(int counterpartyId, AgencyInfo agency);

        Task<Result<AgencyInfo>> GetAgency(int counterpartyId, int agencyId);

        Task<Result<List<AgencyInfo>>> GetAllCounterpartyAgencies(int counterpartyId);

        Task<Agency> GetDefaultAgency(int counterpartyId);

        Task<Result> VerifyAsFullyAccessed(int counterpartyId, string verificationReason);

        Task<Result> VerifyAsReadOnly(int counterpartyId, string verificationReason);
    }
}