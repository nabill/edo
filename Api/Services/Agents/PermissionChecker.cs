using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class PermissionChecker : IPermissionChecker
    {
        public PermissionChecker(EdoContext context, IAdministratorContext administratorContext)
        {
            // TODO: Remove administratorContext from there
            _administratorContext = administratorContext;
            _context = context;
        }


        public async ValueTask<Result> CheckInAgencyPermission(AgentInfo agent, InAgencyPermissions permission)
        {
            //var storedPermissions = await _context.AgentAgencyRelations
            //    .Where(r => r.AgentId == agent.AgentId)
            //    .Where(r => r.AgencyId == agent.AgencyId)
            //    .Select(r => r.InAgencyPermissions)
            //    .SingleOrDefaultAsync();

            //if (Equals(storedPermissions, default))
            //    return Result.Fail("The agent isn't affiliated with the counterparty");

            //return !storedPermissions.HasFlag(permission) 
            //    ? Result.Fail($"Agent does not have permission '{permission}'") 
            //    : Result.Ok();
            // TODO: rework this during permission rework task
            return Result.Ok();
        }
        
        private readonly EdoContext _context;
        private readonly IAdministratorContext _administratorContext;
    }
}