using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgencySystemSettingsManagementService
    {
        Task<Result> SetAvailabilitySearchSettings(int agencyId, AgencyAvailabilitySearchSettings settings);

        Task<Result<AgencyAvailabilitySearchSettings>> GetAvailabilitySearchSettings(int agencyId);

        Task<Result> SetDisplayedPaymentOptions(DisplayedPaymentOptionsSettings settings, int agencyId);

        Task<Result<DisplayedPaymentOptionsSettings>> GetDisplayedPaymentOptions(int agencyId);
    }
}