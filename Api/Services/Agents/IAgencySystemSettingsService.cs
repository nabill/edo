using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgencySystemSettingsService
    {
        Task<Result<AprMode>> GetAdvancedPurchaseRatesSettings(int agencyId);

        Task<Maybe<AgencyAvailabilitySearchSettings>> GetAvailabilitySearchSettings(int agencyId);

        Task<Result<DisplayedPaymentOptionsSettings>> GetDisplayedPaymentOptions(int agencyId, AgentContext agentContext);
    }
}