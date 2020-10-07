using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgencySystemSettingsManagementService
    {
        Task<Result<AprMode>> GetAdvancedPurchaseRatesSettings(int agencyId);

        Task<Result<AgencyAvailabilitySearchSettings>> GetAvailabilitySearchSettings(int agencyId);

        Task<Result<DisplayedPaymentOptionsSettings>> GetDisplayedPaymentOptions(int agencyId);

        Task<Result> SetAdvancedPurchaseRatesSettings(int agencyId, AprMode mode);

        Task<Result> SetAvailabilitySearchSettings(int agencyId, AgencyAvailabilitySearchSettings settings);

        Task<Result> SetDisplayedPaymentOptions(int agencyId, DisplayedPaymentOptionsSettings settings);
    }
}