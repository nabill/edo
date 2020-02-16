using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Management;
using Microsoft.AspNetCore.Authorization;

namespace HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters
{
    public class AdministratorPermissionsAuthorizationHandler : AuthorizationHandler<AdministratorPermissionsAuthorizationRequirement>
    {
        private readonly IAdministratorContext _administratorContext;


        public AdministratorPermissionsAuthorizationHandler(IAdministratorContext administratorContext)
        {
            _administratorContext = administratorContext;
        }


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdministratorPermissionsAuthorizationRequirement requirement)
        {
            var hasPermissions = await _administratorContext.HasPermission(requirement.AdministratorPermissions);
            if(hasPermissions)
                context.Succeed(requirement);
            else
                context.Fail();
        }
    }
}