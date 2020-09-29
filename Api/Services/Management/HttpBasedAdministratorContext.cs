using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Common.Enums;
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

            return await HasGlobalPermission(administrator, permission);
        }


        public async Task<Result<Administrator>> GetCurrent()
        {
            var identity = _tokenInfoAccessor.GetIdentity();
            if (string.IsNullOrWhiteSpace(identity))
                return Result.Failure<Administrator>("Identity is empty");
            
            var identityHash = HashGenerator.ComputeSha256(identity);
            var administrator = await _context.Administrators
                .SingleOrDefaultAsync(c => c.IdentityHash == identityHash);

            if (administrator != default)
                return Result.Success(administrator);

            return Result.Failure<Administrator>("Could not get administrator");
        }


        // TODO: add employee roles
        private Task<bool> HasGlobalPermission(Administrator administrator, AdministratorPermissions permission) => Task.FromResult(true);


        private readonly EdoContext _context;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
    }
}