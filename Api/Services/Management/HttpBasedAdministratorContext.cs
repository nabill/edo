using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Management;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Management
{
    public class HttpBasedAdministratorContext : IAdministratorContext
    {
        public HttpBasedAdministratorContext(EdoContext context, ITokenInfoAccessor tokenInfoAccessor)
        {
            _context = context;
            _tokenInfoAccessor = tokenInfoAccessor;
        }

        public async Task<bool> HasGlobalPermission(GlobalPermissions permission)
        {
            var (_, isFailure, administrator, _) = await GetCurrentAdministrator();
            if (isFailure)
                return false;

            return await HasGlobalPermission(administrator, permission);
        }

        private async Task<Result<Administrator>> GetCurrentAdministrator()
        {
            var identity = _tokenInfoAccessor.GetIdentity();
            if (string.IsNullOrWhiteSpace(identity))
                return Result.Fail<Administrator>("Identity is empty");

            var identityClaim = _tokenInfoAccessor.GetIdentity();
            if (!(identityClaim is null))
            {
                var identityHash = HashGenerator.ComputeHash(identityClaim);
                var administrator = await _context.Administrators
                    .SingleOrDefaultAsync(c => c.IdentityHash == identityHash);

                return Result.Ok(administrator);
            }

            var clientIdClaim = _tokenInfoAccessor.GetClientId();
            if (!(clientIdClaim is null))
            {
#warning TODO: Remove this after implementing client-customer relation
                var administrator = await _context.Administrators
                    .SingleOrDefaultAsync(c => c.IdentityHash == clientIdClaim);

                return Result.Ok(administrator);
            }

            return Result.Fail<Administrator>("Could not get employee");
        }

        private Task<bool> HasGlobalPermission(Administrator administrator, GlobalPermissions permission)
        {
            // TODO: add employee roles
            return Task.FromResult(true);
        }
        
        private readonly EdoContext _context;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
    }
}