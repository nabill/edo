using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.ApiClients;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.ApiClients
{
    /// <summary>
    /// This is temporary implementation based on the agent.
    /// Must be replaced with more valid implementation.
    /// </summary>
    public class ApiClientService : IApiClientService
    {
        public ApiClientService(EdoContext context, IAccommodationBookingSettingsService bookingSettingsService)
        {
            _context = context;
            _bookingSettingsService = bookingSettingsService;
        }
        
        
        public async Task<Result<ApiClientInfo>> GetCurrent(AgentContext agent)
        {
            var doesApiClientExist = await _context.ApiClients
                .AnyAsync(a => a.AgencyId == agent.AgencyId && a.AgentId == agent.AgentId);

            if (!doesApiClientExist)
                return Result.Failure<ApiClientInfo>("Could not get associated API client");

            var settings = await _bookingSettingsService.Get(agent);
            return new ApiClientInfo
            {
                AgencyName = agent.AgencyName,
                EnabledSuppliers = settings.EnabledConnectors,
                HasDirectContractsFilter = settings.AdditionalSearchFilters.HasFlag(SearchFilters.DirectContractsOnly)
            };
        }


        public async Task<GeneratedApiClient> GenerateApiClient(AgentContext agent)
        {
            var apiClient = await _context.ApiClients
                .SingleOrDefaultAsync(a => a.AgencyId == agent.AgencyId && a.AgentId == agent.AgentId);

            var password = PasswordGenerator.Generate();
            var passwordHash = HashGenerator.ComputeSha256(password);
            
            if (Equals(apiClient, default))
            {
                _context.ApiClients.Add(new ApiClient
                {
                    AgencyId = agent.AgencyId,
                    AgentId = agent.AgentId,
                    Name = GenericApiClientName,
                    PasswordHash = passwordHash
                });
            }
            else
            {
                apiClient.Name = GenericApiClientName;
                apiClient.PasswordHash = passwordHash;
                _context.Update(apiClient);
            }

            await _context.SaveChangesAsync();
            
            return new GeneratedApiClient(GenericApiClientName, password);
        }


        private const string GenericApiClientName = "ApiClient";

        private readonly EdoContext _context;
        private readonly IAccommodationBookingSettingsService _bookingSettingsService;
    }
}