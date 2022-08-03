using System.Threading.Tasks;
using Api.Services.Internal;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgencySystemSettingsService : IAgencySystemSettingsService
    {
        public AgencySystemSettingsService(EdoContext context,
            IInternalSystemSettingsService internalSystemSettingsService)
        {
            _context = context;
            _internalSystemSettingsService = internalSystemSettingsService;
        }


        public async Task<Maybe<AgencyAccommodationBookingSettings>> GetAccommodationBookingSettings(int agencyId)
        {
            var (_, isFailure, settings) = await _internalSystemSettingsService.GetAgencyMaterializedSearchSettings(agencyId);
            if (isFailure)
                return Maybe<AgencyAccommodationBookingSettings>.None;

            return settings;
        }


        private readonly EdoContext _context;
        private readonly IInternalSystemSettingsService _internalSystemSettingsService;
    }
}