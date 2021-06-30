using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Management;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AdministratorRolesAssignmentService : IAdministratorRolesAssignmentService
    {
        public AdministratorRolesAssignmentService(EdoContext context,
            IManagementAuditService managementAuditService)
        {
            _context = context;
            _managementAuditService = managementAuditService;
        }


        public async Task<Result> SetAdministratorRoles(int administratorId, List<int> roleIdsList, Administrator assigner)
        {
            return await GetAdministrator()
                .Ensure(AllProvidedRolesExist, "Some of specified roles do not exist")
                .BindWithTransaction(_context, a => Result.Success(a)
                    .Tap(UpdateRoles)
                    .Bind(WriteAuditLog));


            async Task<Result<Administrator>> GetAdministrator()
            {
                var admin = await _context.Administrators
                    .SingleOrDefaultAsync(a => a.Id == administratorId);

                return admin is null
                    ? Result.Failure<Administrator>($"Could not find administrator with id {administratorId}")
                    : Result.Success(admin);
            }


            async Task<bool> AllProvidedRolesExist(Administrator _)
            {
                var allRolesIds = await _context.AdministratorRoles.Select(r => r.Id).ToListAsync();
                return roleIdsList.All(allRolesIds.Contains);
            }


            async Task UpdateRoles(Administrator assignee)
            {
                assignee.AdministratorRoleIds = roleIdsList.ToArray();

                _context.Administrators.Update(assignee);
                await _context.SaveChangesAsync();
            }


            Task<Result> WriteAuditLog(Administrator assignee)
                => _managementAuditService.Write(ManagementEventType.AdministratorRolesAssignment,
                    new AgentRoleAssignmentEventData(
                        assignerAdministratorId: assigner.Id,
                        assigneeAdministratorId: assignee.Id,
                        newRoles: roleIdsList));
        }


        private readonly EdoContext _context;
        private readonly IManagementAuditService _managementAuditService;
    }
}