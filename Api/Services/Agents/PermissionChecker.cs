using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class PermissionChecker : IPermissionChecker
    {
        public PermissionChecker(EdoContext context)
        {
            _context = context;
        }


        public async ValueTask<Result> CheckInAgencyPermission(AgentInfo agent, InAgencyPermissions permission)
        {
            var storedPermissions = await _context.AgentAgencyRelations
                .Where(r => r.AgentId == agent.AgentId)
                .Where(r => r.AgencyId == agent.AgencyId)
                .Select(r => r.InAgencyPermissions)
                .SingleOrDefaultAsync();

            if (Equals(storedPermissions, default))
                return Result.Failure("The agent isn't affiliated with the agency");

            return !storedPermissions.HasFlag(permission)
                ? Result.Failure($"Agent does not have permission '{permission}'")
                : Result.Ok();
        }
        
        private readonly EdoContext _context;
    }
}