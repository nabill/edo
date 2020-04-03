using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class PermissionChecker : IPermissionChecker
    {
        public PermissionChecker(EdoContext context, IAdministratorContext administratorContext)
        {
            // TODO: Remove administratorContext from there
            _administratorContext = administratorContext;
            _context = context;
        }


        public async ValueTask<Result> CheckInCounterpartyPermission(CustomerInfo customer, InCounterpartyPermissions permission)
        {
            var storedPermissions = await _context.CustomerCompanyRelations
                .Where(r => r.CustomerId == customer.CustomerId)
                .Where(r => r.CompanyId == customer.CounterpartyId)
                .Where(r => r.BranchId == customer.BranchId)
                .Select(r => r.InCounterpartyPermissions)
                .SingleOrDefaultAsync();

            if (Equals(storedPermissions, default))
                return Result.Fail("The customer isn't affiliated with the counterparty");

            return !storedPermissions.HasFlag(permission) 
                ? Result.Fail($"Customer does not have permission '{permission}'") 
                : Result.Ok();
        }
        
        private readonly EdoContext _context;
        private readonly IAdministratorContext _administratorContext;
    }
}