using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Settings;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgencySystemSettingsManagementService
    {
        Task<Result<AgencyAccommodationBookingSettings>> GetAvailabilitySearchSettings(int agencyId);
    
        AgencyAccommodationBookingSettings GetAvailabilitySearchSettings(ContractKind? contractKind, AgencyAccommodationBookingSettings? rootSettings, AgencyAccommodationBookingSettings? agencySettings);

        Task<Result> SetAvailabilitySearchSettings(int agencyId, AgencyAccommodationBookingSettingsInfo settings);

        Task<Result> DeleteAvailabilitySearchSettings(int agencyId);
    }
}