using HappyTravel.Edo.Api.Infrastructure;

namespace HappyTravel.Edo.Api.Models.Management
{
    public class ExternalAdminContext : IExternalAdminContext
    {
        private readonly ITokenInfoAccessor _tokenInfoAccessor;

        public ExternalAdminContext(ITokenInfoAccessor tokenInfoAccessor)
        {
            _tokenInfoAccessor = tokenInfoAccessor;
        }
        
        public bool IsExternalAdmin() => _tokenInfoAccessor.GetClientId() == ExternalAdminClientName;

        private const string ExternalAdminClientName = "externalAdmin";
    }
}