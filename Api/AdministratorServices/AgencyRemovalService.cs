using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AgencyRemovalService : IAgencyRemovalService
    {
        public AgencyRemovalService(EdoContext context,
            IManagementAuditService managementAuditService,
            IAgencySystemSettingsManagementService agencySystemSettingsManagementService,
            IAgentRemovalService agentRemovalService)
        {
            _context = context;
            _managementAuditService = managementAuditService;
            _agencySystemSettingsManagementService = agencySystemSettingsManagementService;
            _agentRemovalService = agentRemovalService;
        }
        
        
        public async Task<Result> Delete(int agencyId)
        {
            return await GetAgency(agencyId)
                .Ensure(IsAgencyNotActive, "Can not delete an active agency")
                .Ensure(IsAgencyChildOrNotFullyVerified, "Can not delete a root fully verified agency")
                .Ensure(HasNoBookings, "Can not delete an agency with bookings")
                .BindWithTransaction(_context, agency => Result.Success(agency)
                    .Check(DeleteAgencySystemSettings)
                    .Tap(DeleteApiClients)
                    .Check(DeleteAgentsWhereRequired)
                    .Tap(DeleteAgency)
                    .Tap(WriteAgencyDeletionToAuditLog));


            bool IsAgencyNotActive(Agency agency)
                => !agency.IsActive;


            // If the agency is a child agency, we delete it regardless of verification state.
            bool IsAgencyChildOrNotFullyVerified(Agency agency)
                => agency.ParentId is not null || agency.VerificationState != AgencyVerificationStates.FullAccess;


            async Task<bool> HasNoBookings(Agency agency) 
                => ! await _context.Bookings.AnyAsync(b => b.AgencyId == agencyId);


            Task<Result> DeleteAgencySystemSettings(Agency agency)
                => _agencySystemSettingsManagementService.DeleteAvailabilitySearchSettings(agency.Id);


            async Task DeleteApiClients(Agency agency)
            {
                var apiClients = await _context.ApiClients
                    .Where(c => c.AgencyId == agency.Id)
                    .ToListAsync();

                if (apiClients.Any())
                {
                    _context.RemoveRange(apiClients);
                    await _context.SaveChangesAsync();
                }
            }


            async Task<Result> DeleteAgentsWhereRequired(Agency agency)
            {
                var relations = await _context.AgentAgencyRelations
                    .Where(r => r.AgencyId == agency.Id)
                    .ToListAsync();

                foreach (var relation in relations)
                {
                    var result = await _agentRemovalService.RemoveFromAgency(relation.AgentId, relation.AgencyId);
                    if (result.IsFailure)
                        return result;
                }

                return Result.Success();
            }


            async Task DeleteAgency(Agency agency)
            {
                _context.Remove(agency);
                await _context.SaveChangesAsync();
            }


            Task WriteAgencyDeletionToAuditLog()
                => _managementAuditService.Write(ManagementEventType.AgencyDeletion, new AgencyDeletionEventData(agencyId));
        }


        private async Task<Result<Agency>> GetAgency(int agencyId)
        {
            var agency = await _context.Agencies.FirstOrDefaultAsync(ag => ag.Id == agencyId);
            if (agency == null)
                return Result.Failure<Agency>("Could not find agency with specified id");

            return Result.Success(agency);
        }


        private readonly IManagementAuditService _managementAuditService;
        private readonly IAgencySystemSettingsManagementService _agencySystemSettingsManagementService;
        private readonly IAgentRemovalService _agentRemovalService;
        private readonly EdoContext _context;
    }
}
