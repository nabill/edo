using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AgentRolesManagementService : IAgentRolesManagementService
    {
        public AgentRolesManagementService(EdoContext context)
        {
            _context = context;
        }


        public Task<List<AgentRoleInfo>> GetAll()
            => _context.AgentRoles
                .Select(r => ToAgentRoleInfo(r))
                .ToListAsync();


        public Task<Result> Add(AgentRoleInfo roleInfo)
        {
            return Validate(roleInfo)
                .Ensure(IsUnique, "A role with the same name or permission set already exists")
                .Tap(Add);


            async Task<bool> IsUnique()
                => !await _context.AgentRoles
                    .AnyAsync(r => r.Name.ToLower() == roleInfo.Name.ToLower() || r.Permissions == roleInfo.Permissions.ToFlags());


            Task Add()
            {
                _context.AgentRoles.Add(ToAgentRole(roleInfo));
                return _context.SaveChangesAsync();
            }
        }


        public async Task<Result> Edit(int roleId, AgentRoleInfo roleInfo)
        {
            return await Validate(roleInfo)
                .Ensure(IsUnique, "A role with the same name or permission set already exists")
                .Bind(() => Get(roleId))
                .Tap(Edit);


            async Task<bool> IsUnique()
                => !await _context.AgentRoles
                    .AnyAsync(r => (r.Name.ToLower() == roleInfo.Name.ToLower() || r.Permissions == roleInfo.Permissions.ToFlags())
                        && r.Id != roleId);


            Task Edit(AgentRole role)
            {
                role.Name = roleInfo.Name;
                role.Permissions = roleInfo.Permissions.ToFlags();

                _context.Update(role);
                return _context.SaveChangesAsync();
            }
        }


        public async Task<Result> Delete(int roleId)
        {
            return await Get(roleId)
                .Ensure(IsUnused, "This role is in use and cannot be deleted")
                .Tap(Delete);


            async Task<bool> IsUnused(AgentRole _)
                => !await _context.AgentAgencyRelations.AnyAsync(r => r.AgentRoleIds.Contains(roleId));


            Task Delete(AgentRole role)
            {
                _context.AgentRoles.Remove(role);
                return _context.SaveChangesAsync();
            }
        }


        private async Task<Result<AgentRole>> Get(int roleId)
            => await _context.AgentRoles
                    .SingleOrDefaultAsync(r => r.Id == roleId)
                        ?? Result.Failure<AgentRole>("A role with specified Id does not exist");


        private Result Validate(AgentRoleInfo roleInfo)
            => GenericValidator<AgentRoleInfo>.Validate(v =>
                {
                    v.RuleFor(r => r.Name).NotEmpty();
                    v.RuleFor(r => r.Permissions).NotEmpty();
                },
                roleInfo);
        
        
        private static AgentRoleInfo ToAgentRoleInfo(AgentRole agentRole)
            => new ()
            {
                Id = agentRole.Id,
                Name = agentRole.Name,
                Permissions = agentRole.Permissions.ToList()
            };


        private static AgentRole ToAgentRole(AgentRoleInfo agentRoleInfo)
            => new ()
            {
                Name = agentRoleInfo.Name,
                Permissions = agentRoleInfo.Permissions.ToFlags()
            };


        private readonly EdoContext _context;
    }
}
