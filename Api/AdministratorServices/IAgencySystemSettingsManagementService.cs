using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Settings;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgencySystemSettingsManagementService
    {
        Task<Result<AgencyAccommodationBookingSettings>> GetAvailabilitySearchSettings(int agencyId);

        Task<Result> SetAvailabilitySearchSettings(int agencyId, AgencyAccommodationBookingSettingsInfo settings);

        Task<Result> DeleteAvailabilitySearchSettings(int agencyId);
    }
}