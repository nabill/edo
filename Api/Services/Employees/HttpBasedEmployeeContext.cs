using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Employees;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Employees
{
    public class HttpBasedEmployeeContext : IEmployeeContext
    {
        public HttpBasedEmployeeContext(EdoContext context, ITokenInfoAccessor tokenInfoAccessor)
        {
            _context = context;
            _tokenInfoAccessor = tokenInfoAccessor;
        }
        public async Task<bool> HasGlobalPermission(GlobalPermissions permission)
        {
            var (_, isFailure, employee, _) = await GetCurrentEmployee();
            if (isFailure)
                return false;

            return await HasGlobalPermission(employee, permission);
        }

        private async Task<Result<Employee>> GetCurrentEmployee()
        {
            var identity = _tokenInfoAccessor.GetIdentity();
            if (string.IsNullOrWhiteSpace(identity))
                return Result.Fail<Employee>("Identity is empty");

            var identityClaim = _tokenInfoAccessor.GetIdentity();
            if (!(identityClaim is null))
            {
                var identityHash = HashGenerator.ComputeHash(identityClaim);
                var employee = await _context.Employees
                    .SingleOrDefaultAsync(c => c.IdentityHash == identityHash);
                
                return Result.Ok(employee);
            }

            var clientIdClaim = _tokenInfoAccessor.GetClientId();
            if (!(clientIdClaim is null))
            {
#warning TODO: Remove this after implementing client-customer relation
                var employee = await _context.Employees
                    .SingleOrDefaultAsync(c => c.IdentityHash == clientIdClaim);
                
                return Result.Ok(employee);
            }
            return Result.Fail<Employee>("Could not get employee");
        }

        private Task<bool> HasGlobalPermission(Employee employee, GlobalPermissions permission)
        {
            // TODO: add employee roles
            return Task.FromResult(true);
        }
        
        private readonly EdoContext _context;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
    }
}