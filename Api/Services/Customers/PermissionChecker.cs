using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class PermissionChecker : IPermissionChecker
    {
        public PermissionChecker(EdoContext context, IMemoryFlow flow)
        {
            _context = context;
            _flow = flow;
        }


        public ValueTask<Result> CheckInCompanyPermission(CustomerInfo customer, InCompanyPermissions permission) 
            => CheckPermission(customer, permission, new List<CompanyStates>(1) {CompanyStates.FullAccess});


        public ValueTask<Result> CheckInCompanyReadOnlyPermission(CustomerInfo customer, InCompanyPermissions permission) 
            => CheckPermission(customer, permission, new List<CompanyStates>(2) {CompanyStates.ReadOnly, CompanyStates.FullAccess});


        private async ValueTask<Result> CheckPermission(CustomerInfo customer, InCompanyPermissions permission, List<CompanyStates> states)
        {
            //HACK: there are no possibility to check admin permission, so that's a temporary solution
            var isAdmin = await _context.Customers
                .Join(_context.Administrators, c => c.IdentityHash, a => a.IdentityHash, (c, a) => c.Id)
                .Where(id => id == customer.CustomerId)
                .AnyAsync();
            if (isAdmin)
                return Result.Ok();

            var isCompanyVerified = await IsCompanyHasState(customer.CompanyId, states);
            if (!isCompanyVerified)
                return Result.Fail("The action is available only for verified companies");
            
            var storedPermissions = await _context.CustomerCompanyRelations
                .Where(r => r.CustomerId == customer.CustomerId)
                .Where(r => r.CompanyId == customer.CompanyId)
                .Where(r => r.BranchId == customer.BranchId)
                .Select(r => r.InCompanyPermissions)
                .SingleOrDefaultAsync();

            if (Equals(storedPermissions, default))
                return Result.Fail("The customer isn't affiliated with the company");

            return !storedPermissions.HasFlag(permission) 
                ? Result.Fail($"Customer does not have permission '{permission}'") 
                : Result.Ok();


            ValueTask<bool> IsCompanyHasState(int companyId, List<CompanyStates> companyStates)
            {
                var cacheKey = _flow.BuildKey(nameof(PermissionChecker), nameof(IsCompanyHasState), companyId.ToString());
                return _flow.GetOrSetAsync(cacheKey, ()
                        => _context.Companies
                            .Where(c => c.Id == companyId)
                            .AnyAsync(c => companyStates.Contains(c.State)), 
                    CompanyStateCacheTtl);
            }
        }


        private static readonly TimeSpan CompanyStateCacheTtl = TimeSpan.FromMinutes(5);

        private readonly EdoContext _context;
        private readonly IMemoryFlow _flow;
    }
}