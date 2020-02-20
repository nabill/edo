using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Services.Management;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters
{
    public class AdministratorPermissionsAuthorizationHandler : AuthorizationHandler<AdministratorPermissionsAuthorizationRequirement>
    {
        public AdministratorPermissionsAuthorizationHandler(IAdministratorContext administratorContext,
            ILogger<AdministratorPermissionsAuthorizationHandler> logger)
        {
            _administratorContext = administratorContext;
            _logger = logger;
        }


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdministratorPermissionsAuthorizationRequirement requirement)
        {
            var hasPermissions = await _administratorContext.HasPermission(requirement.AdministratorPermissions);
            if (hasPermissions)
            {
                var adminEmail = (await _administratorContext.GetCurrent()).Value.Email;
                _logger.LogAdministratorAuthorized($"Successfully authorized administrator '{adminEmail}'");
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogAdministratorFailedToAuthorize("Failed to authorize administrator");
                context.Fail();
            }
        }


        private readonly IAdministratorContext _administratorContext;
        private readonly ILogger<AdministratorPermissionsAuthorizationHandler> _logger;
    }
}