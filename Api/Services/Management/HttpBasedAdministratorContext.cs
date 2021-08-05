using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums.Administrators;
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


        public async Task<bool> HasPermission(AdministratorPermissions permission)
        {
            var (_, isFailure, administrator, _) = await GetCurrent();
            if (isFailure)
                return false;

            var availablePermissions = await GetAvailablePermissions(administrator);

            var hasPermission = availablePermissions.HasFlag(permission);

            return hasPermission;
        }


        public async Task<Result<Administrator>> GetCurrent()
        {
            var identity = _tokenInfoAccessor.GetIdentity();
            if (string.IsNullOrWhiteSpace(identity))
                return Result.Failure<Administrator>("Identity is empty");
            
            var identityHash = HashGenerator.ComputeSha256(identity);
            var administrator = await _context.Administrators
                .SingleOrDefaultAsync(c => c.IdentityHash == identityHash && c.IsActive);

            if (administrator != default)
                return Result.Success(administrator);

            return Result.Failure<Administrator>("Could not get administrator");
        }


        private async Task<AdministratorPermissions> GetAvailablePermissions(Administrator administrator)
        {
            var rolesPermissions = await _context.AdministratorRoles
                .Where(x => administrator.AdministratorRoleIds.Contains(x.Id))
                .Select(x => x.Permissions)
                .ToListAsync();

            return rolesPermissions.SelectMany(r => r.ToList()).Aggregate((a, b) => a | b);
        }


        private readonly EdoContext _context;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
    }
}