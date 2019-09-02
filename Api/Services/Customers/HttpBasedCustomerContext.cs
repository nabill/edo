using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class HttpBasedCustomerContext : ICustomerContext
    {
        public HttpBasedCustomerContext(IHttpContextAccessor accessor,
            EdoContext context)
        {
            _accessor = accessor;
            _context = context;
        }

        public async ValueTask<Result<Customer>> GetCustomer()
        {
            _customer = _customer ?? await GetCustomerFromClaims();
            return _customer is null
                ? Result.Fail<Customer>("Could not find customer")
                : Result.Ok(_customer);
        }

        public async ValueTask<Result<Company>> GetCompany()
        {
            _company = _company ?? await GetCompanyFromHttpContext();
            return _company is null
                ? Result.Fail<Company>("Could not find company")
                : Result.Ok(_company);
        }

        public async ValueTask<bool> IsMasterCustomer()
        {
            var (_, isCustomerFailure, customer, _) = await GetCustomer();
            if(isCustomerFailure)
                return false;
            
            var (_, isCompanyFailure, company, _) = await GetCompany();
            if(isCompanyFailure)
                return false;

            return await _context.CustomerCompanyRelations
                .Where(cr => cr.CustomerId == customer.Id)
                .Where(cr => cr.CompanyId == company.Id)
                .Where(cr => cr.Type == CustomerCompanyRelationTypes.Master)
                .AnyAsync();;
        }

        private async ValueTask<Company> GetCompanyFromHttpContext()
        {
            // TODO: implement getting company from headers
            var (_, isFailure, customer, _) = await GetCustomer();
            if (isFailure)
                return null;

            return await _context.CustomerCompanyRelations
                .Where(cr => cr.CustomerId == customer.Id)
                .Join(_context.Companies,
                    relation => relation.CompanyId,
                    company => company.Id,
                    (relation, company) => company)
                .SingleOrDefaultAsync();
        }

        private async ValueTask<Customer> GetCustomerFromClaims()
        {
            var identityClaim = GetCurrentCustomerIdentity();
            if (!(identityClaim is null))
            {
                var identityHash = HashGenerator.ComputeHash(identityClaim);
                return await _context.Customers
                    .SingleOrDefaultAsync(c => c.IdentityHash == identityHash);
            }

            var clientIdClaim = GetClaimValue("client_id");
            if (!(clientIdClaim is null))
            {
#warning TODO: Remove this after implementing client-customer relation
                return await _context.Customers
                    .SingleOrDefaultAsync(c => c.IdentityHash == clientIdClaim);
            }

            return null;
        }

        private string GetCurrentCustomerIdentity()
        {
            return GetClaimValue("sub");
        }

        private string GetClaimValue(string claimType)
        {
            return _accessor.HttpContext.User.Claims
                .SingleOrDefault(c => c.Type == claimType)
                ?.Value;
        }
        
        private readonly IHttpContextAccessor _accessor;
        private readonly EdoContext _context;
        private Company _company;
        private Customer _customer;
    }
}