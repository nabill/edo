using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Agents;

namespace Api.Services.Internal
{
    public interface IInternalSystemSettingsService
    {
        Task<Result<AgentAccommodationBookingSettings>> GetAgentMaterializedSearchSettings(int agentId, int agencyId);
        Task<Result<AgencyAccommodationBookingSettings>> GetAgencyMaterializedSearchSettings(int agencyId);
        AgencyAccommodationBookingSettings GetAgencyMaterializedSearchSettings(ContractKind? contractKind, AgencyAccommodationBookingSettings? rootSettings, AgencyAccommodationBookingSettings? agencySettings);
        Task<Result> CheckRelationExists(int agentId, int agencyId);
    }
}