using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums.Administrators;

namespace HappyTravel.Edo.Api.Services.Management
{
    public class ExternalAdminContext : IExternalAdminContext
    {
        public ExternalAdminContext(ITokenInfoAccessor tokenInfoAccessor)
        {
            _tokenInfoAccessor = tokenInfoAccessor;
        }


        public bool HasPermission(AdministratorPermissions permission)
        {
            // External admins are used only for other administrators invitation for now.
            return permission == AdministratorPermissions.AdministratorInvitation &&
                _tokenInfoAccessor.GetClientId() == ExternalAdminClientName;
        } 


        private const string ExternalAdminClientName = "external_admin";
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
    }
}