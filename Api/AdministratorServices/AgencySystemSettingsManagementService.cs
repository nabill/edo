using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AgencySystemSettingsManagementService : IAgencySystemSettingsManagementService
    {
        public AgencySystemSettingsManagementService(EdoContext context)
        {
            _context = context;
        }

        
        public async Task<Result> SetAvailabilitySearchSettings(int agencyId, AgencyAvailabilitySearchSettings settings)
        {
            var doesAgencyExist = await _context.Agencies.AnyAsync(a => a.Id == agencyId);
            if (!doesAgencyExist)
                return Result.Failure($"Could not find agency with id {agencyId}");

            var existingSettings = await _context.AgencySystemSettings.SingleOrDefaultAsync(s => s.AgencyId == agencyId);
            if (existingSettings == default)
            {
                var newSettings = new AgencySystemSettings
                {
                    AgencyId = agencyId,
                    AvailabilitySearchSettings = settings
                };
                _context.AgencySystemSettings.Add(newSettings);
            }
            else
            {
                existingSettings.AvailabilitySearchSettings = settings;
                _context.Update(existingSettings);
            }

            await _context.SaveChangesAsync();
            return Result.Ok();
        }


        public async Task<Result<AgencyAvailabilitySearchSettings>> GetAvailabilitySearchSettings(int agencyId)
        {
            var doesAgencyExist = await _context.Agencies.AnyAsync(a => a.Id == agencyId);
            if (!doesAgencyExist)
                return Result.Failure<AgencyAvailabilitySearchSettings>($"Could not find agency with id {agencyId}");
            
            var existingSettings = await _context.AgencySystemSettings.SingleOrDefaultAsync(s => s.AgencyId == agencyId);
            return existingSettings == default
                ? Result.Failure<AgencyAvailabilitySearchSettings>($"Could not find availability search settings for agency with id {agencyId}")
                : existingSettings.AvailabilitySearchSettings;
        }


        public async Task<Result> SetDisplayedPaymentOptions(DisplayedPaymentOptionsSettings settings, int agencyId)
        {
            return await Result.Success()
                .Ensure(() => IsAgencyExist(agencyId), "Agency with such id does not exist")
                .Tap(SetOptions);


            async Task SetOptions()
            {
                var systemSettings = await _context.AgencySystemSettings.SingleOrDefaultAsync(s => s.AgencyId == agencyId)
                    ?? new AgencySystemSettings { AgencyId = agencyId };

                systemSettings.DisplayedPaymentOptions = settings;
                _context.Update(systemSettings);

                await _context.SaveChangesAsync();
            }
        }


        public async Task<Result<DisplayedPaymentOptionsSettings>> GetDisplayedPaymentOptions(int agencyId)
        {
            return await Result.Success()
                .Ensure(() => IsAgencyExist(agencyId), "Agency with such id does not exist")
                .Bind(GetOptions);


            async Task<Result<DisplayedPaymentOptionsSettings>> GetOptions()
            {
                var systemSettings = await _context.AgencySystemSettings.SingleOrDefaultAsync(s => s.AgencyId == agencyId);
                var options = systemSettings?.DisplayedPaymentOptions;

                return options == null
                    ? Result.Failure<DisplayedPaymentOptionsSettings>("No value found for DisplayedPaymentOptions settings")
                    : Result.Success(options.Value);
            }
        }


        private Task<bool> IsAgencyExist(int agencyId) => _context.Agencies.AnyAsync(a => a.Id == agencyId && a.IsActive);


        private readonly EdoContext _context;
    }
}